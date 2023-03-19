using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Dalamud.Game.Command;
using Dalamud.Plugin;
using Dalamud.Game.Gui.FlyText;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using KamiLib;
using KamiLib.ChatCommands;
using Newtonsoft.Json;
using Tf2CriticalHitsPlugin.Common.Configuration;
using Tf2CriticalHitsPlugin.Configuration;
using Tf2CriticalHitsPlugin.Countdown;
using Tf2CriticalHitsPlugin.Countdown.Status;
using Tf2CriticalHitsPlugin.Countdown.Windows;
using Tf2CriticalHitsPlugin.CriticalHits.Configuration;
using Tf2CriticalHitsPlugin.CriticalHits.Windows;
using Tf2CriticalHitsPlugin.SeFunctions;
using Tf2CriticalHitsPlugin.Windows;
using static Dalamud.Logging.PluginLog;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Tf2CriticalHitsPlugin
{
    public sealed class Tf2CriticalHitsPlugin : IDalamudPlugin
    {
        public string Name => "TF2-ish Critical Hits";
        private const string CommandName = "/critconfig";

        public ConfigTwo Configuration { get; init; }
        
        
        public readonly WindowSystem WindowSystem = new("TF2CriticalHitsPlugin");
        internal static PlaySound? GameSoundPlayer;
        private readonly CountdownModule countdownModule;

        public Tf2CriticalHitsPlugin(DalamudPluginInterface pluginInterface)
        {
            pluginInterface.Create<Service>();
            KamiCommon.Initialize(pluginInterface, Name, () => Configuration?.Save());
            
            Configuration = InitConfig();
            Configuration.Save();
            

            KamiCommon.WindowManager.AddWindow(new ConfigWindow(Configuration));
            KamiCommon.WindowManager.AddWindow(new CriticalHitsCopyWindow(Configuration.criticalHits));
            KamiCommon.WindowManager.AddWindow(new CriticalHitsImportWindow(Configuration.criticalHits));
            KamiCommon.WindowManager.AddWindow(new CountdownNewSettingWindow(Configuration.countdownJams));

            countdownModule = new CountdownModule(State.Instance(), Configuration.countdownJams);

            
            GameSoundPlayer = new PlaySound(Service.SigScanner);

            Service.CommandManager.AddHandler(CommandName, new CommandInfo(OnConfigCommand)
            {
                HelpMessage = "Opens the TF2-ish Critical Hits configuration window"
            });

            Service.PluginInterface.UiBuilder.Draw += DrawUserInterface;
            Service.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigWindow;

            Service.FlyTextGui.FlyTextCreated += this.FlyTextCreate;
        }

        private static ConfigTwo InitConfig()
        {
            var configFile = Service.PluginInterface.ConfigFile.FullName;
            if (!File.Exists(configFile))
            {
                return new ConfigTwo();
            }

            var configText = File.ReadAllText(configFile);
            var unixTimeSeconds = DateTimeOffset.Now.ToUnixTimeSeconds();

            try
            {
                var versionCheck = JsonSerializer.Deserialize<BaseConfiguration>(configText);
                if (versionCheck is null)
                {
                    return new ConfigTwo();
                }

                var version = versionCheck.Version;
                var config = version switch
                {
                    0 => JsonSerializer.Deserialize<CriticalHitsConfigZero>(configText)?.MigrateToOne().MigrateToTwo(versionCheck.PluginVersion) ?? new ConfigTwo(),
                    1 => JsonConvert.DeserializeObject<CriticalHitsConfigOne>(configText)?.MigrateToTwo(versionCheck.PluginVersion) ?? new ConfigTwo(),
                    2 => JsonSerializer.Deserialize<ConfigTwo>(configText) ?? new ConfigTwo(),
                    _ => new ConfigTwo()
                };

                TriggerChatAlertsForEarlierVersions(config);
                
                
                // For testing only
                Service.PluginInterface.ConfigFile.MoveTo(Service.PluginInterface.ConfigFile.FullName + $".{unixTimeSeconds}.old", true);

                return config;
            }
            catch (Exception e)
            {
                if (e.StackTrace is not null) LogError(e.StackTrace);
                // var unixTimeSeconds = DateTimeOffset.Now.ToUnixTimeSeconds();
                // Service.PluginInterface.ConfigFile.MoveTo(Service.PluginInterface.ConfigFile.FullName + $".{unixTimeSeconds}.old", true);
                Chat.PrintError(
                    $"There was an error while reading your configuration file and it was reset. The old file is available in your pluginConfigs folder, as Tf2CriticalHitsPlugin.json.{unixTimeSeconds}.old.");
                return new ConfigTwo();
            }
        }

        private static void TriggerChatAlertsForEarlierVersions(BaseConfiguration config)
        {
            if (config.PluginVersion.Before(2, 0, 0))
            {
                Chat.Print("Update 2.0.0.0", "Long time no see! Now you can configure Critical Heals, use the game's sound effects and separate configurations per job. Open /critconfig to check!");
            }
            if (config.PluginVersion.Before(2, 1, 0))
            {
                Chat.Print("Update 2.1.0.0", "Now you can configure Critical Heals from your job separately from Critical Heals done by other players' jobs!");
            }
            if (config.PluginVersion.Before(2, 2, 0))
            {
                Chat.Print("Update 2.2.0.0", "New volume settings have been added for v2.2.0.0, which are enabled by default. If you're using a custom sound and it's too low, open /critconfig and adjust.");
            }

            if (config.PluginVersion.Before(3, 0, 0))
            {
                Chat.Print("Update 3.0.0.0", "New module: Countdown Jams! Configure a sound to be played when a countdown begins... and if it's cancelled.");
            }
        }

        public void FlyTextCreate(
            ref FlyTextKind kind,
            ref int val1,
            ref int val2,
            ref SeString text1,
            ref SeString text2,
            ref uint color,
            ref uint icon,
            ref uint damageTypeIcon,
            ref float yOffset,
            ref bool handled)
        {
            var currentText2 = text2.ToString();
            var currentClassJobId = currentText2.StartsWith("TF2TEST##")
                                        ? byte.Parse(
                                            currentText2[
                                                (currentText2.LastIndexOf("#", StringComparison.Ordinal) + 1)..])
                                        : GetCurrentClassJobId();
            if (currentClassJobId is null) return;

            foreach (var config in Configuration.criticalHits.JobConfigurations[currentClassJobId.Value])
            {
                if (ShouldTriggerInCurrentMode(config) &&
                    (IsAutoAttack(config, kind) ||
                     IsEnabledAction(config, kind, text1, currentClassJobId)))
                {
                    LogDebug($"{config.GetId()} registered!");
                    if (config.ShowText)
                    {
                        text2 = GenerateText(config);
                    }

                    if (!config.SoundForActionsOnly ||
                        config.GetModuleDefaults().FlyTextType.Action.Contains(kind))
                    {
                        if (config.UseCustomFile)
                        {
                            SoundEngine.PlaySound(config.FilePath.Value, config.ApplySfxVolume, config.Volume.Value);
                        }
                        else
                        {
                            GameSoundPlayer?.Play(config.GameSound.Value);
                        }
                    }
                }
            }
        }

        private static bool ShouldTriggerInCurrentMode(CriticalHitsConfigOne.ConfigModule config)
        {
            return !IsPvP() || config.ApplyInPvP;
        }
        
        private static bool IsAutoAttack(CriticalHitsConfigOne.ConfigModule config, FlyTextKind kind)
        {
            return config.GetModuleDefaults().FlyTextType.AutoAttack.Contains(kind);
        }

        private static bool IsEnabledAction(
            CriticalHitsConfigOne.ConfigModule config, FlyTextKind kind, SeString text, [DisallowNull] byte? currentClassJobId)
        {
            // If it's not a FlyText for an action, return false
            if (!config.GetModuleDefaults().FlyTextType.Action.Contains(kind)) return false;
            // If we're checking the Own Critical Heals section, check if it's an action of the current job
            if (config.ModuleType == ModuleType.OwnCriticalHeal)
            {
                return Constants.ActionsPerJob[currentClassJobId.Value].Contains(text.TextValue);
            }
            // If we're checking the Other Critical Heals section, check if it's NOT an action of the current job
            if (config.ModuleType == ModuleType.OtherCriticalHeal)
            {
                return !Constants.ActionsPerJob[currentClassJobId.Value].Contains(text.TextValue);
            }
            // If it's any other configuration section, it's enabled.
            return true;
        }

        private static unsafe byte? GetCurrentClassJobId()
        {
            var classJobId = PlayerState.Instance()->CurrentClassJobId;
            return Constants.CombatJobs.ContainsKey(classJobId) ? classJobId : null;
        }

        private static bool IsPvP()
        {
            return Service.ClientState.IsPvP;
        }
        
        public static void GenerateTestFlyText(CriticalHitsConfigOne.ConfigModule config)
        {
            var kind = config.GetModuleDefaults().FlyTextType.Action.FirstOrDefault();
            LogDebug($"Kind: {kind}, Config ID: {config.GetId()}");
            var text = GetTestText(config);
            Service.FlyTextGui.AddFlyText(kind, 1, 3333, 0, new SeStringBuilder().AddText(text).Build(),
                                          new SeStringBuilder().AddText($"TF2TEST##{config.ClassJobId}").Build(),
                                          config.GetModuleDefaults().FlyTextColor, 0, 60012);
        }

        private static string GetTestText(CriticalHitsConfigOne.ConfigModule configModule)
        {
            var array = configModule.ModuleType.Value == ModuleType.OwnCriticalHeal
                            ? Constants.ActionsPerJob[configModule.ClassJobId.Value].ToArray()
                            : Constants.TestFlavorText;
            return array[(int)Math.Floor(Random.Shared.NextSingle() * array.Length)];
        }

        private static SeString GenerateText(CriticalHitsConfigOne.ConfigModule config)
        {
            LogDebug(
                $"Generating text with color {config.TextColor} and glow {config.TextGlowColor}");
            var stringBuilder = new SeStringBuilder()
                                .AddUiForeground(config.TextColor.Value)
                                .AddUiGlow(config.TextGlowColor.Value);
            if (config.TextItalics)
            {
                stringBuilder.AddItalicsOn();
            }

            return stringBuilder
                   .AddText(config.Text.Value)
                   .AddItalicsOff()
                   .AddUiForegroundOff()
                   .AddUiGlowOff()
                   .Build();
        }


        public void Dispose()
        {
            KamiCommon.Dispose();
            countdownModule.Dispose();
            Service.FlyTextGui.FlyTextCreated -= FlyTextCreate;
            this.WindowSystem.RemoveAllWindows();
            Service.CommandManager.RemoveHandler(CommandName);
        }

        private static void OnConfigCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            if (KamiCommon.WindowManager.GetWindowOfType<ConfigWindow>() is { } window)
            {
                window.IsOpen = true;
            }
        }

        private void DrawUserInterface()
        {
            this.WindowSystem.Draw();
        }

        public static void DrawConfigWindow()
        {
            if (KamiCommon.WindowManager.GetWindowOfType<ConfigWindow>() is { } window)
            {
                window.IsOpen = true;
            }
        }
    }
}
