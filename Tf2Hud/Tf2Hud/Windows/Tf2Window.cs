using System;
using System.Numerics;
using Dalamud.Interface.Raii;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace Tf2Hud.Tf2Hud.Windows;

public abstract class Tf2Window : Window
{
    public Team Team { get; set; }
    public const int ScorePanelWidth = 270;
    public const int ScorePanelHeight = 65;
    protected const int MvpListHeight = 280;

    protected Tf2Window(string name, Team team) : base(
        name,
        ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoResize)
    {
        Team = team;
        PositionCondition = ImGuiCond.Always;
        BgAlpha = 0.8f;
    }

    protected static ImFontPtr Tf2Font { get; private set; }
    protected static ImFontPtr Tf2ScoreFont { get; private set; }
    protected static ImFontPtr Tf2SecondaryFont { get; private set; }

    private ImRaii.Color? windowBgColor;
    private ImRaii.Color? borderColor;
    
    public override void PreDraw()
    {
        Service.Log($"Tf2Window - PostDraw");
        windowBgColor = ImRaii.PushColor(ImGuiCol.WindowBg, Team.BgColor);
        borderColor = ImRaii.PushColor(ImGuiCol.Border, new Vector4(255, 255, 255, 20));
    }

    public override void PostDraw()
    {
        borderColor?.Dispose();
        windowBgColor?.Dispose();
        Service.Log($"Tf2Window - PostDraw ${GetType()}");
    }
    
    public static void UpdateFontPointers(ImFontPtr tf2Font, ImFontPtr tf2ScoreFont, ImFontPtr tf2SecondaryFont)
    {
        Tf2Font = tf2Font;
        Tf2ScoreFont = tf2ScoreFont;
        Tf2SecondaryFont = tf2SecondaryFont;
    }

}
