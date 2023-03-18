using Dalamud.Configuration;
using Tf2CriticalHitsPlugin.Configuration;

namespace Tf2CriticalHitsPlugin.Common.Configuration;

public class BaseConfiguration: IPluginConfiguration
{
    public int Version { get; set; } = -1;
    public PluginVersion PluginVersion { get; set; } = PluginVersion.From(0, 0, 0);
}
