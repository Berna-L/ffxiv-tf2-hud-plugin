using System.Numerics;
using ImGuiNET;
using Tf2Hud.Common.Windows;

namespace Tf2Hud.Tf2Hud.Windows;

public class Tf2BluScore : Tf2Window
{
    public Tf2BluScore() : base("##Tf2BluScore", TeamColor.Blu.Background)
    {
        Size = new Vector2(ScorePanelWidth, 65);
    }

    public int Score { get; set; } = 0;


    public override void Draw()
    {
        ImGui.PushFont(Tf2SecondaryFont);
        ImGuiHelper.TextShadow("BLU");
        ImGui.PopFont();
        var calcTextSize = ImGuiHelper.CalcTextSize(Tf2ScoreFont, Score.ToString());
        ImGuiHelper.ForegroundTextShadow(Tf2ScoreFont, Score.ToString(),
                                         ImGui.GetCursorScreenPos() + ImGui.GetContentRegionAvail() with { Y = -85 } -
                                         calcTextSize with { Y = 0 });
        ImGui.GetWindowDrawList();
    }
}
