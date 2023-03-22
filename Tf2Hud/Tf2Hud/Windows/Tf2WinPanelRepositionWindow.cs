using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using ImGuiNET;
using KamiLib;
using KamiLib.Drawing;

namespace Tf2Hud.Tf2Hud.Windows;

public class Tf2WinPanelRepositionWindow: Window
{
    public Tf2WinPanelRepositionWindow() : base("##Tf2WinPanelRepositionWindow", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize)
    {
        Size = new Vector2(Tf2Window.ScorePanelWidth * 2, Tf2Window.ScorePanelHeight + Tf2Window.MvpListHeight);
        PositionCondition = ImGuiCond.Appearing;
        Position = GetPosition();
        BgAlpha = 0.5f;
    }

    private static Vector2? GetPosition()
    {
        return KamiCommon.WindowManager.GetWindowOfType<Tf2BluScoreWindow>()?.Position;
    }

    public override void PreDraw()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
        ImGui.PushStyleColor(ImGuiCol.Border, Colors.Black);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, Colors.Black);
    }

    public override void OnOpen()
    {
        PluginLog.Debug("abri");
    }

    public override void OnClose()
    {
        Position = GetPosition();
    }

    public override void PostDraw()
    {
        ImGui.PopStyleColor();
        ImGui.PopStyleColor();
        ImGui.PopStyleVar();
    }

    public override void Draw()
    {
    }
}
