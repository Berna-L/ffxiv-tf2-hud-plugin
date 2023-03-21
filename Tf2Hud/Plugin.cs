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
using static Dalamud.Logging.PluginLog;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Tf2Hud;

public sealed class Plugin : IDalamudPlugin
{
    public const string PluginName = "Team Fortress 2 HUD";
    private const string CommandName = "/tf2hudconfig";

    private readonly Tf2HudModule? tf2HudModule;


    public readonly WindowSystem WindowSystem = new("Tf2Hud");
    private readonly DalamudPluginInterface dalamudPluginInterface;


    public Plugin(DalamudPluginInterface pluginInterface)
    {
        dalamudPluginInterface = pluginInterface;
        dalamudPluginInterface.Create<Service>();
        KamiCommon.Initialize(dalamudPluginInterface, Name, () => Configuration?.Save());
        tf2HudModule = new Tf2HudModule();

        var config = (dalamudPluginInterface.GetPluginConfig() as ConfigZero) ?? new ConfigZero();

        KamiCommon.WindowManager.AddWindow(new ConfigWindow(config));
        
        dalamudPluginInterface.UiBuilder.RebuildFonts();

        Configuration = InitConfig();
        Configuration.Save();


        KamiCommon.WindowManager.AddWindow(new ConfigWindow(Configuration));

        

        Service.CommandManager.AddHandler(CommandName, new CommandInfo(OnConfigCommand)
        {
            HelpMessage = "Opens the TF2 HUD configuration window"
        });


        Service.PluginInterface.UiBuilder.Draw += DrawUserInterface;
        Service.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigWindow;
    }

    public ConfigZero Configuration { get; init; }
    public string Name => PluginName;

    public void Dispose()
    {
        KamiCommon.Dispose();
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

            var version = versionCheck.Version;
            var config = version switch
            {
                0 => JsonConvert.DeserializeObject<ConfigZero>(configText) ?? new ConfigZero(),
                _ => new ConfigZero()
            };
            

            // For testing only
            Service.PluginInterface.ConfigFile.MoveTo(
                Service.PluginInterface.ConfigFile.FullName + $".{unixTimeSeconds}.old", true);

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
