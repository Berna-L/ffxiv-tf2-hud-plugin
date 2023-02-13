using Dalamud.Data;
using Dalamud.Game.Command;
using Dalamud.Game.Gui.FlyText;
using Dalamud.IoC;
using Dalamud.Plugin;

namespace Tf2CriticalHitsPlugin;

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
}
