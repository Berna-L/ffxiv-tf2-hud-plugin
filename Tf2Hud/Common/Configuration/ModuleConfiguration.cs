using System;
using Dalamud.Configuration;
using KamiLib.Configuration;

namespace Tf2Hud.Common.Configuration;

public abstract class ModuleConfiguration : IPluginConfiguration
{
    public int Version { get; set; }

    public Setting<bool> Enabled { get; set; } = new(true);

    [NonSerialized]
    public Setting<bool> RepositionMode = new(false);

}
