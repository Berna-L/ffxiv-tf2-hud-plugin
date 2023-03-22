using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace Tf2Hud.Tf2Hud.Windows;

public abstract class Tf2Window : Window
{
    public Team Team { get; set; }
    protected const int ScorePanelWidth = 270; 

    protected Tf2Window(string name, Team team) : base(
        name,
        ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoResize)
    {
        Team = team;
        BgAlpha = 0.8f;
    }

    protected static ImFontPtr Tf2Font { get; private set; }
    protected static ImFontPtr Tf2ScoreFont { get; private set; }
    protected static ImFontPtr Tf2SecondaryFont { get; private set; }

    
    public override void PreDraw()
    {
        ImGui.PushStyleColor(ImGuiCol.WindowBg, Team.BgColor);
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

}
