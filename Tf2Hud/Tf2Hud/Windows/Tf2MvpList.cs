using System.Linq;
using System.Numerics;
using Dalamud.Interface.GameFonts;
using ImGuiNET;
using KamiLib.Drawing;
using Pilz.Dalamud.Icons;
using Tf2Hud.Common;
using Tf2Hud.Common.Windows;

namespace Tf2Hud.Tf2Hud.Windows;

public class Tf2MvpList : Tf2Window
{
    private readonly JobIconSets jobIconSets;
    private readonly GameFontHandle playerNameFont;
    private const int ScorePanelHeight = 280;


    public Tf2MvpList() : base("##Tf2MvpList", TeamColor.Red.Background)
    {
        Size = new Vector2(ScorePanelWidth * 2, ScorePanelHeight);
        jobIconSets = new JobIconSets();
        playerNameFont = Service.PluginInterface.UiBuilder.GetGameFontHandle(new GameFontStyle(GameFontFamily.Axis, 18));
    }

    public string? LastEnemy { get; set; }

    public override void PreDraw()
    {
        base.PreDraw();
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 0f);
    }

    public override void Draw()
    {
        var team = BackgroundColor == TeamColor.Blu.Background ? "BLU" : "RED";
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
        ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.85f);
        ImGui.PushStyleColor(ImGuiCol.FrameBg, Colors.Black.ToU32());
        ImGui.BeginChildFrame(12313, new Vector2(InnerFrameWidth, InnerFrameHeight));
        ImGui.PopStyleColor();
        ImGui.PopStyleVar();
        var jobIcon = jobIconSets.GetJobIcon(JobIconSetName.Framed,
                                             Constants.CombatJobs.First(cj => cj.Value.NameEnglish == "Scholar").Key);
        var imGuiHandle = Service.DataManager?.GetImGuiTextureIcon((uint)jobIcon)?.ImGuiHandle;
        ImGui.Text("BLU team MVPs:");
        ImGui.SameLine();
        var pointsThisRound = "(everyone is an MVP :)";
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X -
                            ImGui.CalcTextSize(pointsThisRound).X);
        ImGui.Text(pointsThisRound);
        ImGui.GetWindowDrawList().AddLine(ImGui.GetCursorScreenPos() + new Vector2(0, 0),
                                          ImGui.GetCursorScreenPos() + new Vector2(InnerFrameWidth, 0), Colors.White.ToU32());
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 10);
        var middlePosX = ImGui.GetCursorPosX() + (ImGui.GetContentRegionAvail().X / 2);
        if (imGuiHandle is not null)
        {
            ImGui.PushFont(playerNameFont.ImFont);
            for (int i = 0; i < 4; i++)
            {
                ImGui.Image(imGuiHandle.Value, new Vector2(32, 32));
                ImGui.SameLine();
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 6);
                ImGui.TextColored(TeamColor.Blu.Text, "Namesheavy Weaponsguy");
                ImGui.SameLine();
                ImGui.SetCursorPosX(middlePosX);
                ImGui.Image(imGuiHandle.Value, new Vector2(32, 32));
                ImGui.SameLine();
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 6);
                ImGui.TextColored(TeamColor.Blu.Text, "Namesheavy Weaponsguy");
            }
            ImGui.PopFont();
        }

        // ImGui.Text("Highest Killstreak:");
        // ImGui.SameLine();
        // var count = "Count:";
        // ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(count).X);
        // ImGui.Text(count);
        // ImGui.GetWindowDrawList().AddLine(ImGui.GetCursorScreenPos() + new Vector2(0, 0),
        //                                   ImGui.GetCursorScreenPos() + new Vector2(490, 0), Colors.White.ToU32());
        // ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 10);
        // if (imGuiHandle is not null) ImGui.Image(imGuiHandle.Value, new Vector2(40, 40));
        ImGui.EndChildFrame();
    }

    private static int InnerFrameWidth => (ScorePanelWidth * 2) - 21;
    private static int InnerFrameHeight => ScorePanelHeight - 85;


    public override void PostDraw()
    {
        ImGui.PopStyleVar();
        base.PostDraw();
    }
}
