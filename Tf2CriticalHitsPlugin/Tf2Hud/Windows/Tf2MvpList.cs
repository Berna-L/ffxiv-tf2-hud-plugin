using System.Numerics;
using ImGuiNET;
using Tf2CriticalHitsPlugin.Common.Windows;

namespace Tf2CriticalHitsPlugin.Tf2Hud.Windows;

public class Tf2MvpList: Tf2Window
{
    
    public Tf2MvpList() : base("##Tf2DetailWindow", TeamColor.Red)
    {
        Size = new Vector2(258 * 2, 300);
        BgAlpha = 0.8f;
    }

    public string LastEnemy { get; set; }

    public override void PreDraw()
    {
        base.PreDraw();
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 0f);
    }

    public override void Draw()
    {
        var team = BackgroundColor == TeamColor.Blu ? "BLU" : "RED";
        var teamWinMessage = $"{team} TEAM WINS!";
        ImGui.PushFont(Tf2SecondaryFont);
        ImGui.SetCursorPosX((ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(teamWinMessage).X) / 2);
        ImGuiHelper.TextShadow(teamWinMessage);
        ImGui.PopFont();
        var winConditionMessage = team == "BLU"
                                      ? $"BLU defeated {LastEnemy} before the time ran out"
                                      : $"BLU was wiped by {LastEnemy}";
        ImGui.SetCursorPosX((ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(winConditionMessage).X) / 2);
        ImGui.Text(winConditionMessage);
        ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.5f);
        ImGui.BeginChildFrame(12313, new Vector2(490, 200));
        ImGui.EndChildFrame();
        ImGui.PopStyleVar();
    }

    
    public override void PostDraw()
    {
        ImGui.PopStyleVar();
        base.PostDraw();
    }
}
