using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using Dalamud.Game.Command;
using Dalamud.Plugin;
using Dalamud.Game.Gui.FlyText;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using KamiLib;
using KamiLib.ChatCommands;
using Lumina.Excel.GeneratedSheets;
using Tf2CriticalHitsPlugin.Configuration;
using Tf2CriticalHitsPlugin.GameSettings;
using Tf2CriticalHitsPlugin.SeFunctions;
using Tf2CriticalHitsPlugin.Windows;
using static Dalamud.Logging.PluginLog;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;

namespace Tf2CriticalHitsPlugin
{
    public sealed class Tf2CriticalHitsPlugin : IDalamudPlugin
    {
        public string Name => "TF2-ish Critical Hits";
        private const string CommandName = "/critconfig";

        public ConfigOne Configuration { get; init; }
        public readonly WindowSystem WindowSystem = new("TF2CriticalHitsPlugin");
        internal static PlaySound? GameSoundPlayer;

        public Tf2CriticalHitsPlugin(DalamudPluginInterface pluginInterface)
        {
            pluginInterface.Create<Service>();
            KamiCommon.Initialize(pluginInterface, Name, () => Configuration?.Save());

            InitColors();

            this.Configuration = InitConfig();
            Configuration.Save();

            KamiCommon.WindowManager.AddWindow(new ConfigWindow(this));
            KamiCommon.WindowManager.AddWindow(new SettingsCopyWindow(Configuration));

            GameSoundPlayer = new PlaySound(Service.SigScanner);

            Service.CommandManager.AddHandler(CommandName, new CommandInfo(OnConfigCommand)
            {
                HelpMessage = "Opens the TF2-ish Critical Hits configuration window"
            });

            Service.PluginInterface.UiBuilder.Draw += DrawUserInterface;
            Service.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigWindow;

            Service.FlyTextGui.FlyTextCreated += this.FlyTextCreate;
        }

        private static ConfigOne InitConfig()
        {
            var configFile = Service.PluginInterface.ConfigFile.FullName;
            if (!File.Exists(configFile))
            {
                return new ConfigOne();
            }

            var configText = File.ReadAllText(configFile);
            try
            {
                var versionCheck = JsonSerializer.Deserialize<VersionCheck>(configText);
                if (versionCheck is null)
                {
                    return new ConfigOne();
                }

                var version = versionCheck.Version;
                var config = version switch
                {
                    0 => JsonSerializer.Deserialize<ConfigZero>(configText)?.MigrateToOne() ?? new ConfigOne(),
                    1 => Service.PluginInterface.GetPluginConfig() as ConfigOne ?? new ConfigOne(),
                    _ => new ConfigOne()
                };

                return config;
            }
            catch (Exception e)
            {
                if (e.StackTrace is not null) LogError(e.StackTrace);
                Service.PluginInterface.ConfigFile.MoveTo(Service.PluginInterface.ConfigFile.FullName + ".old", true);
                Chat.PrintError(
                    "There was an error while reading your configuration file and it was reset. The old file is available in your pluginConfigs folder, as Tf2CriticalHitsPlugin.json.old.");
                return new ConfigOne();
            }
        }


        private static void InitColors()
        {
            ConfigWindow.ForegroundColors.Clear();
            ConfigWindow.GlowColors.Clear();

            if (Service.DataManager != null)
            {
                var colorSheet = Service.DataManager.GetExcelSheet<UIColor>();
                if (colorSheet != null)
                {
                    for (var i = 0u; i < colorSheet.RowCount; i++)
                    {
                        var row = colorSheet.GetRow(i);
                        if (row != null)
                        {
                            ConfigWindow.ForegroundColors.Add(
                                (ushort)i, ColorInfo.FromUiColor((ushort)i, row.UIForeground));
                            ConfigWindow.GlowColors.Add((ushort)i, ColorInfo.FromUiColor((ushort)i, row.UIGlow));
                        }
                    }
                }
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

            foreach (var config in Configuration.JobConfigurations[currentClassJobId.Value])
            {
                if (IsSameType(config, color) &&
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
                            var modifier = config.ApplySfxVolume ? GetEffectiveSfxVolume() : 1f;
                            SoundEngine.PlaySound(config.FilePath.Value, config.Volume.Value * modifier * 0.01f);
                        }
                        else
                        {
                            GameSoundPlayer?.Play(config.GameSound.Value);
                        }
                    }
                }
            }
        }

        private static bool IsSameType(ConfigOne.ConfigModule config, uint color)
        {
            return config.GetModuleDefaults().FlyTextColor == color;
        }

        private static bool IsAutoAttack(ConfigOne.ConfigModule config, FlyTextKind kind)
        {
            return config.GetModuleDefaults().FlyTextType.AutoAttack.Contains(kind);
        }

        private static bool IsEnabledAction(
            ConfigOne.ConfigModule config, FlyTextKind kind, SeString text, [DisallowNull] byte? currentClassJobId)
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

        private static float GetEffectiveSfxVolume()
        {
            if (GameConfig.System.GetBool(ConfigOption.IsSndSe) ||
                GameConfig.System.GetBool(ConfigOption.IsSndMaster))
            {
                return 0;
            }
            return GameConfig.System.GetUInt(ConfigOption.SoundSe)/100f * (GameConfig.System.GetUInt(ConfigOption.SoundMaster)/100f);
        }

        public static void GenerateTestFlyText(ConfigOne.ConfigModule config)
        {
            var kind = config.GetModuleDefaults().FlyTextType.Action.FirstOrDefault();
            LogDebug($"Kind: {kind}, Config ID: {config.GetId()}");
            var text = GetTestText(config);
            Service.FlyTextGui.AddFlyText(kind, 1, 3333, 0, new SeStringBuilder().AddText(text).Build(),
                                          new SeStringBuilder().AddText($"TF2TEST##{config.ClassJobId}").Build(),
                                          config.GetModuleDefaults().FlyTextColor, 0, 60012);
        }

        private static string GetTestText(ConfigOne.ConfigModule configModule)
        {
            var array = configModule.ModuleType.Value == ModuleType.OwnCriticalHeal
                            ? Constants.ActionsPerJob[configModule.ClassJobId.Value].ToArray()
                            : Constants.TestFlavorText;
            return array[(int)Math.Floor(Random.Shared.NextSingle() * array.Length)];
        }

        private static SeString GenerateText(ConfigOne.ConfigModule config)
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
