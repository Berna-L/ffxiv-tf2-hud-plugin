using System;
using KamiLib.Configuration;

namespace Tf2CriticalHitsPlugin.Configuration;

public class FlyTextParameters: ICloneable
{
    public Setting<ushort> ColorKey { get; set; }
    public Setting<ushort> GlowColorKey { get; set; }
    public Setting<bool> Italics { get; set; }

    public FlyTextParameters(ushort colorKey, ushort glowColorKey, bool italics)
    {
        ColorKey = new Setting<ushort>(colorKey);
        GlowColorKey = new Setting<ushort>(glowColorKey);
        Italics = new Setting<bool>(italics);
    }

    public object Clone()
    {
        return new FlyTextParameters(ColorKey.Value, GlowColorKey.Value, Italics.Value);
    }
}
