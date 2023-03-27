using System.Numerics;
using Dalamud.Interface.Raii;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Tf2Hud.Common.Model;

namespace Tf2Hud.Tf2Hud.Windows;

public abstract class Tf2Window : Window
{
    public const int ScorePanelWidth = 270;
    public const int ScorePanelHeight = 65;
    protected const int MvpListHeight = 280;
    public static Vector4 TanLight = new(235 / 255f, 226 / 255f, 202 / 255f, 255 / 255f);
    private ImRaii.Color? borderColor;
    private ImRaii.Style? borderStyle;

    private ImRaii.Color? windowBgColor;
    private ImRaii.Style? windowRounding;

    protected Tf2Window(string name, Tf2Team team) : base(
        name,
        ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMouseInputs)
    {
        Team = team;
        PositionCondition = ImGuiCond.Always;
        BgAlpha = 0.8f;
    }

    public Tf2Team Team { get; set; }

    protected static ImFontPtr Tf2Font { get; private set; }
    protected static ImFontPtr Tf2ScoreFont { get; private set; }
    protected static ImFontPtr Tf2SecondaryFont { get; private set; }

    public override void PreDraw()
    {
        windowBgColor = ImRaii.PushColor(ImGuiCol.WindowBg, Team.BgColor);
        borderColor = ImRaii.PushColor(ImGuiCol.Border, TanLight);
        borderStyle = ImRaii.PushStyle(ImGuiStyleVar.WindowBorderSize, 3);
        windowRounding = ImRaii.PushStyle(ImGuiStyleVar.WindowRounding, 5);
    }

    public override void PostDraw()
    {
        windowRounding?.Dispose();
        borderStyle?.Dispose();
        borderColor?.Dispose();
        windowBgColor?.Dispose();
    }

    public static void UpdateFontPointers(ImFontPtr tf2Font, ImFontPtr tf2ScoreFont, ImFontPtr tf2SecondaryFont)
    {
        Tf2Font = tf2Font;
        Tf2ScoreFont = tf2ScoreFont;
        Tf2SecondaryFont = tf2SecondaryFont;
    }
}
