using System.Numerics;

namespace Tf2Hud.Common.Windows;

// Copied from https://github.com/Lerofni/TooltipNotes/blob/main/TooltipNotes/Windows/ConfigWindow.cs#L83,
// by Lerofni and mrexodia
public class ColorInfo
{
    public byte A;
    public byte B;
    public byte G;
    public ushort Index = ushort.MaxValue;
    public byte R;

    public Vector4 Vec4 => new(R / 255f, G / 255f, B / 255f, A / 255f);

    public static ColorInfo FromUiColor(ushort index, uint foreground)
    {
        return new ColorInfo
        {
            Index = index,
            R = (byte)((foreground >> 24) & 0xFF),
            G = (byte)((foreground >> 16) & 0xFF),
            B = (byte)((foreground >> 8) & 0xFF),
            A = (byte)((foreground >> 0) & 0xFF)
        };
    }

    public override string ToString()
    {
        return $"#{R:X2}{G:X2}{B:X2}:{Index}";
    }
}
