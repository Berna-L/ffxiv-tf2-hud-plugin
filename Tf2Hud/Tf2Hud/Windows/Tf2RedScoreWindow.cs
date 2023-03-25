using System.Numerics;
using Dalamud.Interface.Raii;
using ImGuiNET;
using Tf2Hud.Common.Windows;

namespace Tf2Hud.Tf2Hud.Windows;

public class Tf2RedScoreWindow : Tf2TeamScoreWindow
{

    public Tf2RedScoreWindow() : base("##Tf2RedScore", Team.Red)
    {
        Size = new Vector2(ScorePanelWidth, ScorePanelHeight);
    }
    
    public override void Draw()
    {
        using (ImRaii.PushFont(Tf2SecondaryFont))
        {
            if (Size != null) ImGui.SetCursorPosX(ImGui.GetContentRegionAvail().X + 10 - ImGui.CalcTextSize(Team.Name).X);
            ImGuiHelper.TextShadow(Team.Name);
        }
        ImGuiHelper.ForegroundTextShadow(Tf2ScoreFont, Score.ToString(),
                                         ImGui.GetCursorScreenPos() + new Vector2(10, -85));
    }
}
