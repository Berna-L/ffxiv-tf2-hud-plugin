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
        pluginInterface.Create<Service>();
        KamiCommon.Initialize(Service.PluginInterface, Name, () => Config?.Save());
        Config = InitConfig();

        Config.Save();

        tf2HudModule = new Tf2HudModule(Config);
        tf2VoiceLinesModule = new Tf2VoiceLinesModule(Config.General, Config.VoiceLines);

        // if (Config.PluginVersion.Before(0, 1, 0))
        // {
        //     Config.General.UpdateFromOldVersion(Config);
        // }

        KamiCommon.WindowManager.AddWindow(new ConfigWindow(Config));

        Service.PluginInterface.UiBuilder.RebuildFonts();


        Service.CommandManager.AddHandler(CommandName, new CommandInfo(OnConfigCommand)
        {
            HelpMessage = "Opens the Team Fortress Fantasy configuration window"
        });


        Service.PluginInterface.UiBuilder.Draw += DrawUserInterface;
        Service.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigWindow;
    }

    public ConfigZero Config { get; init; }
    public string Name => PluginName;

    public void Dispose()
    {
        KamiCommon.Dispose();
        tf2VoiceLinesModule?.Dispose();
        ;
        tf2HudModule?.Dispose();
        WindowSystem.RemoveAllWindows();
        Service.CommandManager.RemoveHandler(CommandName);
    }


    private static ConfigZero InitConfig()
    {
        var configFile = Service.PluginInterface.ConfigFile.FullName;
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


            if (Service.PluginInterface.IsTesting || Service.PluginInterface.IsDev)
            {
                Service.PluginInterface.ConfigFile.MoveTo(
                    Service.PluginInterface.ConfigFile.FullName + $".{unixTimeSeconds}.old", true);
            }

            return config;
        }
        catch (Exception e)
        {
            if (e.StackTrace is not null) LogError(e.StackTrace);
            // var unixTimeSeconds = DateTimeOffset.Now.ToUnixTimeSeconds();
            // Service.PluginInterface.ConfigFile.MoveTo(Service.PluginInterface.ConfigFile.FullName + $".{unixTimeSeconds}.old", true);
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
