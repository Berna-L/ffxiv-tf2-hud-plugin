using System;
using System.Numerics;
using Dalamud.Configuration;
using KamiLib.Configuration;

namespace Tf2Hud.Common.Configuration;

public abstract class ModuleConfiguration : IPluginConfiguration
{
    public int Version { get; set; }

    public Setting<bool> Enabled { get; set; } = new(true);
    public Setting<float> PositionX { get; set; }
    public Setting<float> PositionY { get; set; }

    [NonSerialized]
    public Setting<bool> RepositionMode = new(false);

    public ModuleConfiguration()
    {
        PositionX = new Setting<float>(GetPositionXDefault());
        PositionY = new Setting<float>(GetPositionYDefault());
    }

    public Vector2 GetPosition() => new Vector2(PositionX.Value, PositionY.Value);

    public abstract float GetPositionXDefault();
    public abstract float GetPositionYDefault();

    public void RestoreDefaultPosition()
    {
        PositionX.Value = GetPositionXDefault();
        PositionY.Value = GetPositionYDefault();
    }
}
