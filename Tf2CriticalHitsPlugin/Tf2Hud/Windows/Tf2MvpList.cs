using System.Linq;
using System.Numerics;
using ImGuiNET;
using KamiLib.Drawing;
using Pilz.Dalamud.Icons;
using Tf2CriticalHitsPlugin.Common.Windows;

namespace Tf2CriticalHitsPlugin.Tf2Hud.Windows;

public class Tf2MvpList: Tf2Window
{
    private readonly JobIconSets jobIconSets;


    public Tf2MvpList() : base("##Tf2DetailWindow", TeamColor.Red)
    {
        Size = new Vector2(258 * 2, 340);
        jobIconSets = new JobIconSets();
    }

    public string? LastEnemy { get; set; } = null;

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
        ImGui.BeginChildFrame(12313, new Vector2(495, 255));
        ImGui.PopStyleVar();
        var jobIcon = jobIconSets.GetJobIcon(JobIconSetName.Framed, Constants.CombatJobs.First(cj => cj.Value.NameEnglish == "Scholar").Key);
        var imGuiHandle = Service.DataManager?.GetImGuiTextureIcon((uint)jobIcon)?.ImGuiHandle;
        ImGui.Text($"BLU team MVPs:");
        ImGui.SameLine();
        var pointsThisRound = "Points this round:";
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(pointsThisRound).X);
        ImGui.Text(pointsThisRound);
        ImGui.GetWindowDrawList().AddLine(ImGui.GetCursorScreenPos() + new Vector2(0, 0), ImGui.GetCursorScreenPos() + new Vector2(490, 0), Colors.White.ToU32());
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 10);
        // ImGui.GetWindowDrawList().PathFillConvex(Colors.White.ToU32());
        if (imGuiHandle is not null)
        {
            ImGui.Image(imGuiHandle.Value, new Vector2(40, 40));
            ImGui.Image(imGuiHandle.Value, new Vector2(40, 40));
            ImGui.Image(imGuiHandle.Value, new Vector2(40, 40));

        }
        ImGui.Text($"Highest Killstreak:");
        ImGui.SameLine();
        var count = "Count:";
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(count).X);
        ImGui.Text(count);
        ImGui.GetWindowDrawList().AddLine(ImGui.GetCursorScreenPos() + new Vector2(0, 0), ImGui.GetCursorScreenPos() + new Vector2(490, 0), Colors.White.ToU32());
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 10);
        if (imGuiHandle is not null)
        {
            ImGui.Image(imGuiHandle.Value, new Vector2(40, 40));

        }
        ImGui.EndChildFrame();
    }

    
    public override void PostDraw()
    {
        ImGui.PopStyleVar();
        base.PostDraw();
    }
}
