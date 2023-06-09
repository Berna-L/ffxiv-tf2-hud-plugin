﻿using System;
using System.Numerics;
using KamiLib.Configuration;

namespace Tf2Hud.Common.Configuration;

public abstract class WindowModuleConfiguration : ModuleConfiguration
{
    [NonSerialized]
    public Setting<bool> RepositionMode = new(false);

    public WindowModuleConfiguration()
    {
        PositionX = new Setting<float>(GetPositionXDefault());
        PositionY = new Setting<float>(GetPositionYDefault());
    }

    public Setting<float> PositionX { get; set; }
    public Setting<float> PositionY { get; set; }

    public Setting<float> Scale { get; set; } = new(1.0f);

    public Vector2 GetPosition()
    {
        return new Vector2(PositionX.Value, PositionY.Value);
    }

    public abstract float GetPositionXDefault();
    public abstract float GetPositionYDefault();

    public void RestoreDefaultPosition()
    {
        PositionX.Value = GetPositionXDefault();
        PositionY.Value = GetPositionYDefault();
    }
}
