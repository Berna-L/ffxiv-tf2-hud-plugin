using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace Tf2Hud.Tf2Hud.Windows;

public abstract class Tf2Window : Window
{
    public Vector4 BackgroundColor;

    protected Tf2Window(string name, Vector4 backgroundColor) : base(
        name,
        ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoResize)
    {
        BackgroundColor = backgroundColor;
        BgAlpha = 0.8f;
    }

    protected static ImFontPtr Tf2Font { get; private set; }
    protected static ImFontPtr Tf2ScoreFont { get; private set; }
    protected static ImFontPtr Tf2SecondaryFont { get; private set; }

    public override void PreDraw()
    {
        ImGui.PushStyleColor(ImGuiCol.WindowBg, BackgroundColor);
        ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(255, 255, 255, 20));
    }

    public override void PostDraw()
    {
        ImGui.PopStyleColor();
        ImGui.PopStyleColor();
    }
    
    public static void UpdateFontPointers(ImFontPtr tf2Font, ImFontPtr tf2ScoreFont, ImFontPtr tf2SecondaryFont)
    {
        Tf2Font = tf2Font;
        Tf2ScoreFont = tf2ScoreFont;
        Tf2SecondaryFont = tf2SecondaryFont;
    }

    public static class TeamColor
    {
        internal static Vector4 Red = new(184 / 255f, 56 / 255f, 59 / 255f, 0);
        internal static Vector4 Blu = new(88 / 255f, 133 / 255f, 162 / 255f, 0);
    }
}
