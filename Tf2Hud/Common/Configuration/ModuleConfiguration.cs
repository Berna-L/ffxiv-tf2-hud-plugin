using Dalamud.Configuration;
using KamiLib.Configuration;

namespace Tf2Hud.Common.Configuration;

public abstract class ModuleConfiguration : IPluginConfiguration
{
    public Setting<bool> Enabled { get; protected init; } = new(true);
    public int Version { get; set; }
}
