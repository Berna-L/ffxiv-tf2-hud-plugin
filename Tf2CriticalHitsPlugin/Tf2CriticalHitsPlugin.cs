using System;
using System.IO;
using Dalamud.Game.Command;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using KamiLib;
using KamiLib.ChatCommands;
using Newtonsoft.Json;
using Tf2CriticalHitsPlugin.Common.Configuration;
using Tf2CriticalHitsPlugin.Common.Windows;
using Tf2CriticalHitsPlugin.Countdown;
using Tf2CriticalHitsPlugin.Countdown.Status;
using Tf2CriticalHitsPlugin.Countdown.Windows;
using Tf2CriticalHitsPlugin.CriticalHits;
using Tf2CriticalHitsPlugin.CriticalHits.Configuration;
using Tf2CriticalHitsPlugin.CriticalHits.Windows;
using Tf2CriticalHitsPlugin.Tf2Hud;
using Tf2CriticalHitsPlugin.Tf2Hud.Windows;
using Tf2CriticalHitsPlugin.Windows;
using static Dalamud.Logging.PluginLog;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Tf2CriticalHitsPlugin
{
    public sealed class Tf2CriticalHitsPlugin : IDalamudPlugin
    {
        public string Name => PluginName;
        public const string PluginName = "Hit it, Joe!";
        private const string CommandName = "/joeconfig";
        private const string LegacyCommandName = "/critconfig";

        public ConfigTwo Configuration { get; init; }
        
        
        public readonly WindowSystem WindowSystem = new("TF2CriticalHitsPlugin");
        
        private readonly CriticalHitsModule criticalHitsModule;
        private readonly CountdownModule countdownModule;
        private readonly Tf2HudModule tf2HudModule;
        private DalamudPluginInterface dalamudPluginInterface;

        private ImFontPtr tf2Font;
        private ImFontPtr tf2ScoreFont;
        private ImFontPtr tf2SecondaryFont;
        private readonly Tf2WinPanel tf2WinPanel;

        public Tf2CriticalHitsPlugin(DalamudPluginInterface pluginInterface)
        {
            dalamudPluginInterface = pluginInterface;
            dalamudPluginInterface.UiBuilder.BuildFonts += LoadFonts;
            dalamudPluginInterface.UiBuilder.RebuildFonts();
            dalamudPluginInterface.Create<Service>();
            KamiCommon.Initialize(dalamudPluginInterface, Name, () => Configuration?.Save());
            
            Configuration = InitConfig();
            Configuration.Save();
            

            KamiCommon.WindowManager.AddWindow(new ConfigWindow(Configuration));
            KamiCommon.WindowManager.AddWindow(new CriticalHitsCopyWindow(Configuration.criticalHits));
            KamiCommon.WindowManager.AddWindow(new CriticalHitsImportWindow(Configuration.criticalHits));
            KamiCommon.WindowManager.AddWindow(new CountdownNewSettingWindow(Configuration.countdownJams));


            criticalHitsModule = new CriticalHitsModule(Configuration.criticalHits);
            countdownModule = new CountdownModule(State.Instance(), Configuration.countdownJams);
            tf2HudModule = new Tf2HudModule();


            Service.CommandManager.AddHandler(CommandName, new CommandInfo(OnConfigCommand)
            {
                HelpMessage = "Opens the Hit it, Joe! configuration window",
            });
            
            Service.CommandManager.AddHandler(LegacyCommandName, new CommandInfo(OnConfigCommand)
            {
                ShowInHelp = false
            });

            Service.PluginInterface.UiBuilder.Draw += DrawUserInterface;
            Service.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigWindow;
            
            // var package = new Package();
            // package.Read(
            //     "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Team Fortress 2\\tf\\tf2_sound_misc_dir.vpk");
            // PluginLog.Debug(package.Entries?.ToString() ?? string.Empty);

        }

        private void LoadFonts()
        {
            tf2Font = ImGui.GetIO().Fonts
                           .AddFontFromFileTTF(
                               "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Team Fortress 2\\tf\\resource\\tf2.ttf",
                               60
                           );

            tf2ScoreFont = ImGui.GetIO().Fonts
                           .AddFontFromFileTTF(
                               "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Team Fortress 2\\tf\\resource\\tf2.ttf",
                               130
                           );
            tf2SecondaryFont = ImGui.GetIO().Fonts
                                    .AddFontFromFileTTF(
                                        "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Team Fortress 2\\tf\\resource\\tf2secondary.ttf",
                                        40);
            Tf2Window.UpdateFontPointers(tf2Font, tf2ScoreFont, tf2SecondaryFont);
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
                    2 => JsonConvert.DeserializeObject<ConfigTwo>(configText) ?? new ConfigTwo(),
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
                Chat.Print("Update 3.0.0.0", "TF2-ish Critical Hits has been renamed to Hit it Joe, and comes with a new module: Countdown Jams! Configure a sound to be played when a countdown begins and if it's cancelled.");
            }
        }
        
        public void Dispose()
        {
            KamiCommon.Dispose();
            dalamudPluginInterface.UiBuilder.BuildFonts -= LoadFonts;
            tf2HudModule.Dispose();
            countdownModule.Dispose();
            criticalHitsModule.Dispose();
            this.WindowSystem.RemoveAllWindows();
            Service.CommandManager.RemoveHandler(LegacyCommandName);
            Service.CommandManager.RemoveHandler(CommandName);
        }

        private static void OnConfigCommand(string command, string args)
        {
            if (command.Equals(LegacyCommandName))
            {
                Chat.Print("Deprecated Command", "The command /critconfig is deprecated and will be removed in the future. Use /joeconfig from now on.");
            }
            if (KamiCommon.WindowManager.GetWindowOfType<ConfigWindow>() is { } window)
            {
                window.IsOpen = true;
            }
        }

        private void OnTestCommand(string command, string args)
        {
            tf2WinPanel.IsOpen = true;

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
