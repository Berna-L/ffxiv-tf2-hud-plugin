using System.Numerics;
using ImGuiNET;
using Tf2Hud.Common.Windows;

namespace Tf2Hud.Tf2Hud.Windows;

public class Tf2BluScoreWindow : Tf2TeamScoreWindow
{
    private static readonly Vector2 DefaultPosition = new((ImGui.GetMainViewport().Size.X / 2) - ScorePanelWidth, ImGui.GetMainViewport().Size.Y - 500);

    public Tf2BluScoreWindow() : base("##Tf2BluScore", Team.Blu)
    {
        Size = new Vector2(ScorePanelWidth, ScorePanelHeight);
        Position = DefaultPosition;
    }

    public override void Draw()
    {
        ImGui.PushFont(Tf2SecondaryFont);
        ImGuiHelper.TextShadow(Team.Name);
        ImGui.PopFont();
        var calcTextSize = ImGuiHelper.CalcTextSize(Tf2ScoreFont, Score.ToString());
        ImGuiHelper.ForegroundTextShadow(Tf2ScoreFont, Score.ToString(),
                                         ImGui.GetCursorScreenPos() + ImGui.GetContentRegionAvail() with { Y = -85 } -
                                         calcTextSize with { Y = 0 });
        ImGui.GetWindowDrawList();
    }
}
