using Dalamud.Configuration;
using KamiLib.Configuration;

namespace Tf2Hud.Common.Configuration;

public abstract class ModuleConfiguration : IPluginConfiguration
{
    public Setting<bool> Enabled { get; set; } = new(true);
    public int Version { get; set; }
}
