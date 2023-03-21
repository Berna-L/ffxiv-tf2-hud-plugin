using System.Numerics;
using ImGuiNET;
using Tf2Hud.Tf2Hud.Windows;

namespace Tf2Hud.Common.Windows;

public class Tf2RedScore : Tf2Window
{
    public Tf2RedScore() : base("##Tf2RedScore", TeamColor.Red.Background)
    {
        Size = new Vector2(ScorePanelWidth, 65);
    }

    public int Score { get; set; } = 0;

    public override void Draw()
    {
        ImGui.PushFont(Tf2SecondaryFont);
        if (Size != null) ImGui.SetCursorPosX(ImGui.GetContentRegionAvail().X + 10 - ImGui.CalcTextSize("RED").X);
        ImGuiHelper.TextShadow("RED");
        ImGui.PopFont();
        ImGuiHelper.ForegroundTextShadow(Tf2ScoreFont, Score.ToString(),
                                         ImGui.GetCursorScreenPos() + new Vector2(10, -85));
        ImGui.GetWindowDrawList();
    }
}
