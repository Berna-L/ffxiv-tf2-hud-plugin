using System;
using System.IO;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.Command;
using Dalamud.Game.DutyState;
using Dalamud.Game.Gui.FlyText;
using Dalamud.IoC;
using Dalamud.Plugin;

namespace Tf2Hud;

public class Service
{
    [PluginService]
    public static DalamudPluginInterface PluginInterface { get; private set; } = null!;

    [PluginService]
    public static CommandManager CommandManager { get; private set; } = null!;

    [PluginService]
    public static FlyTextGui FlyTextGui { get; private set; } = null!;

    [PluginService]
    public static DataManager? DataManager { get; private set; } = null;

    [PluginService]
    public static SigScanner SigScanner { get; private set; } = null!;

    [PluginService]
    public static ClientState ClientState { get; private set; } = null!;

    [PluginService]
    public static Condition Condition { get; private set; } = null!;

    [PluginService]
    public static Framework Framework { get; private set; } = null!;

    [PluginService]
    public static DutyState DutyState { get; private set; } = null!;

    [PluginService]
    public static ObjectTable ObjectTable { get; private set; } = null!;

    [PluginService]
    public static PartyList PartyList { get; private set; } = null!;

    internal static readonly string FileName = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
    
}

public static class LogExtension
{
    public static void Log(this object obj, string log)
    {
        var time = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK");
        var logLocation = Path.Combine(Service.PluginInterface.GetPluginConfigDirectory(),
                                       $"{Service.PluginInterface.InternalName}.{Service.FileName}.log");
        using var streamWriter = File.AppendText(logLocation);
        streamWriter.WriteLine($"[{time}] [{obj.GetType().Name}] {log}");
        streamWriter.Close();
    }

}
