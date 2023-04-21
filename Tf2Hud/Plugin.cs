using System;
using System.IO;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using KamiLib;
using KamiLib.ChatCommands;
using Newtonsoft.Json;
using Tf2Hud.Common.Configuration;
using Tf2Hud.Common.Windows;
using Tf2Hud.Tf2Hud;
using Tf2Hud.VoiceLines;
using static Dalamud.Logging.PluginLog;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Tf2Hud;

public sealed class Plugin : IDalamudPlugin
{
    public const string PluginName = "Team Fortress Fantasy";
    private const string CommandName = "/tfconfig";

    private readonly Tf2HudModule? tf2HudModule;
    private readonly Tf2VoiceLinesModule? tf2VoiceLinesModule;


    public readonly WindowSystem WindowSystem = new("TeamFortressFantasy");


    public Plugin(DalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Services>();
        pluginInterface.Create<CriticalCommonLib.Service>();

        Services.Initialize();
        KamiCommon.Initialize(CriticalCommonLib.Service.Interface, Name, () => Config?.Save());
        Config = InitConfig();

        TriggerChatAlertsForEarlierVersions(Config);
        
        Config.Save();

        tf2HudModule = new Tf2HudModule(Config);
        tf2VoiceLinesModule = new Tf2VoiceLinesModule(Config.General, Config.VoiceLines);

        KamiCommon.WindowManager.AddWindow(new ConfigWindow(Config));

        CriticalCommonLib.Service.Interface.UiBuilder.RebuildFonts();

        CriticalCommonLib.Service.Commands.AddHandler(CommandName, new CommandInfo(OnConfigCommand)
        {
            HelpMessage = "Opens the Team Fortress Fantasy configuration window"
        });


        CriticalCommonLib.Service.Interface.UiBuilder.Draw += DrawUserInterface;
        CriticalCommonLib.Service.Interface.UiBuilder.OpenConfigUi += DrawConfigWindow;
    }

    private void TriggerChatAlertsForEarlierVersions(ConfigZero config)
    {
        if (config.PluginVersion.Before(1, 1, 0))
        {
            Chat.Print("Update 1.1.0.0", "Now, you can set the Win Panel to save your scores per duty, even if you close the game. Open /tfconfig to start!");
        }
    }

    public ConfigZero Config { get; init; }
    public string Name => PluginName;

    public void Dispose()
    {
        KamiCommon.Dispose();
        tf2VoiceLinesModule?.Dispose();
        tf2HudModule?.Dispose();
        WindowSystem.RemoveAllWindows();
        Services.Dispose();
        CriticalCommonLib.Service.Commands.RemoveHandler(CommandName);
    }


    private static ConfigZero InitConfig()
    {
        var configFile = CriticalCommonLib.Service.Interface.ConfigFile.FullName;
        if (!File.Exists(configFile)) return new ConfigZero();

        var configText = File.ReadAllText(configFile);
        var unixTimeSeconds = DateTimeOffset.Now.ToUnixTimeSeconds();

        try
        {
            var versionCheck = JsonSerializer.Deserialize<BaseConfiguration>(configText);
            if (versionCheck is null) return new ConfigZero();
            LogDebug("read base");

            var version = versionCheck.Version;
            Debug($"Version {version.ToString()}");
            var config = version switch
            {
                0 => JsonConvert.DeserializeObject<ConfigZero>(configText) ?? new ConfigZero(),
                _ => new ConfigZero()
            };


            if (CriticalCommonLib.Service.Interface.IsTesting || CriticalCommonLib.Service.Interface.IsDev)
            {
                CriticalCommonLib.Service.Interface.ConfigFile.MoveTo(
                    CriticalCommonLib.Service.Interface.ConfigFile.FullName + $".{unixTimeSeconds}.old", true);
            }

            return config;
        }
        catch (Exception e)
        {
            if (e.StackTrace is not null) LogError(e.StackTrace);
            Chat.PrintError(
                $"There was an error while reading your configuration file and it was reset. The old file is available in your pluginConfigs folder, as Tf2Hud.json.{unixTimeSeconds}.old.");
            return new ConfigZero();
        }
    }

    private static void OnConfigCommand(string command, string args)
    {
        if (KamiCommon.WindowManager.GetWindowOfType<ConfigWindow>() is { } window) window.IsOpen = true;
    }

    private void DrawUserInterface()
    {
        WindowSystem.Draw();
    }

    public static void DrawConfigWindow()
    {
        if (KamiCommon.WindowManager.GetWindowOfType<ConfigWindow>() is { } window) window.IsOpen = true;
    }
}
