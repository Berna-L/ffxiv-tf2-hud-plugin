using System.Numerics;
using Dalamud.Interface.Raii;
using ImGuiNET;
using Tf2Hud.Common.Model;
using Tf2Hud.Common.Windows;

namespace Tf2Hud.Tf2Hud.Windows;

public class Tf2BluScoreWindow : Tf2TeamScoreWindow
{
    public Tf2BluScoreWindow() : base("##Tf2BluScore", Tf2Team.Blu)
    {
        Size = new Vector2(ScorePanelWidth, ScorePanelHeight);
    }

    public override void Draw()
    {
        using (ImRaii.PushFont(Tf2SecondaryFont))
        {
            ImGuiHelper.TextShadow(Team.Name);
        }

        var calcTextSize = ImGuiHelper.CalcTextSize(Tf2ScoreFont, Score.ToString());
        ImGuiHelper.ForegroundTextShadow(Tf2ScoreFont, Score.ToString(),
                                         ImGui.GetCursorScreenPos() + ImGui.GetContentRegionAvail() with { Y = -85 } -
                                         calcTextSize with { Y = 0 });
    }
}
