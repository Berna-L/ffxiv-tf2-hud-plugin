using System.Numerics;
using ImGuiNET;
using Tf2Hud.Common.Windows;

namespace Tf2Hud.Tf2Hud.Windows;

public class Tf2RedScoreWindow : Tf2TeamScoreWindow
{
    public Tf2RedScoreWindow() : base("##Tf2RedScore", Team.Red)
    {
        Size = new Vector2(ScorePanelWidth, 65);
    }
    
    public override void Draw()
    {
        ImGui.PushFont(Tf2SecondaryFont);
        if (Size != null) ImGui.SetCursorPosX(ImGui.GetContentRegionAvail().X + 10 - ImGui.CalcTextSize(Team.Name).X);
        ImGuiHelper.TextShadow(Team.Name);
        ImGui.PopFont();
        ImGuiHelper.ForegroundTextShadow(Tf2ScoreFont, Score.ToString(),
                                         ImGui.GetCursorScreenPos() + new Vector2(10, -85));
        ImGui.GetWindowDrawList();
    }
}
