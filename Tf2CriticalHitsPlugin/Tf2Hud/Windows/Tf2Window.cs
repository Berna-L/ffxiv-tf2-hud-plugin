using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace Tf2CriticalHitsPlugin.Common.Windows;

public abstract class Tf2Window: Window
{
    public static class Color
    {
        public static Vector4 Red = new Vector4(184 / 255f, 56 / 255f, 59 / 255f, 0);
        public static Vector4 Blu = new Vector4(88 / 255f, 133 / 255f, 162 / 255f, 0);
    }
    
    public static ImFontPtr Tf2Font { get; private set; }
    protected static ImFontPtr Tf2ScoreFont { get; private set; }
    protected static ImFontPtr Tf2SecondaryFont { get; private set; }
    private readonly Vector4 backgroundColor;

    protected Tf2Window(string name, Vector4 backgroundColor) : base(name, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar)
    {
        this.backgroundColor = backgroundColor;
        BgAlpha = 0.8f;
    }

    public override void PreDraw()
    {
        ImGui.PushStyleColor(ImGuiCol.WindowBg, backgroundColor);
        ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(255, 255, 255, 20));
    }

    public override void PostDraw()
    {
        ImGui.PopStyleColor();
        ImGui.PopStyleColor();
    }

    public override void Draw()
    {
        throw new System.NotImplementedException();
    }
    
    public static void UpdateFontPointers(ImFontPtr tf2Font, ImFontPtr tf2ScoreFont, ImFontPtr tf2SecondaryFont)
    {
        Tf2Font = tf2Font;
        Tf2ScoreFont = tf2ScoreFont;
        Tf2SecondaryFont = tf2SecondaryFont;
    }

}
