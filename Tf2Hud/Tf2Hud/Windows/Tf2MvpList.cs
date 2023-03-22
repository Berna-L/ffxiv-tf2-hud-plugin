﻿using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.GameFonts;
using ImGuiNET;
using KamiLib.Drawing;
using Pilz.Dalamud.Icons;
using Tf2Hud.Common.Windows;

namespace Tf2Hud.Tf2Hud.Windows;

public class Tf2MvpList : Tf2Window
{
    private readonly JobIconSets jobIconSets;
    private readonly GameFontHandle playerNameFont;
    private readonly Vector2 classJobIconSize = new(32, 32);
    private const int ScorePanelHeight = 280;


    public Tf2MvpList() : base("##Tf2MvpList", Team.Red)
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
        var teamWinMessage = $"{WinningTeam.Name} TEAM WINS!";
        ImGui.PushFont(Tf2SecondaryFont);
        ImGui.SetCursorPosX((ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(teamWinMessage).X) / 2);
        ImGuiHelper.TextShadow(teamWinMessage);
        ImGui.PopFont();
        var winConditionMessage = WinningTeam == PlayerTeam
                                      ? $"{WinningTeam.Name} defeated {LastEnemy} before the time ran out"
                                      : $"{WinningTeam.Name}, led by {LastEnemy}, wiped {WinningTeam.Enemy.Name}";
        ImGui.SetCursorPosX((ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(winConditionMessage).X) / 2);
        ImGui.Text(winConditionMessage);
        ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.85f);
        ImGui.PushStyleColor(ImGuiCol.FrameBg, Colors.Black.ToU32());
        ImGui.BeginChildFrame(12313, new Vector2(InnerFrameWidth, InnerFrameHeight));
        ImGui.PopStyleColor();
        ImGui.PopStyleVar();
        ImGui.Text($"{PlayerTeam.Name} team MVPs:");
        ImGui.SameLine();
        var pointsThisRound = "(everyone is an MVP :)";
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X -
                            ImGui.CalcTextSize(pointsThisRound).X);
        ImGui.Text(pointsThisRound);
        ImGui.GetWindowDrawList().AddLine(ImGui.GetCursorScreenPos() + new Vector2(0, 0),
                                          ImGui.GetCursorScreenPos() + new Vector2(InnerFrameWidth, 0), Colors.White.ToU32());
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 10);
        var middlePosX = ImGui.GetCursorPosX() + (ImGui.GetContentRegionAvail().X / 2);
        ImGui.PushFont(playerNameFont.ImFont);
        for (var i = 0; i < PartyList.Count; i++)
        {
            if (PartyList.Count <= i) break; 
            var leftPartyMember = PartyList[i];
            if (leftPartyMember is null) break;
            ImGui.Image(GetClassJobIcon(leftPartyMember.ClassJobId)!.Value, classJobIconSize);
            ImGui.SameLine();
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 6);
            ImGui.TextColored(WinningTeam.TextColor, leftPartyMember.Name);
            if (PartyList.Count > 4 && PartyList.Count > i + 1)
            {
                var rightPartyMember = PartyList[i + 1];
                if (rightPartyMember is null) continue;
                i++;
                ImGui.SameLine();
                ImGui.SetCursorPosX(middlePosX);
                ImGui.Image(GetClassJobIcon(rightPartyMember.ClassJobId)!.Value, classJobIconSize);
                ImGui.SameLine();
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 6);
                ImGui.TextColored(WinningTeam.TextColor, rightPartyMember.Name);
            }
        }
        ImGui.PopFont();
        ImGui.EndChildFrame();
        
    }

    private nint? GetClassJobIcon(uint id)
    {
        var iconId = jobIconSets.GetJobIcon(JobIconSetName.Framed, id);
        return Service.DataManager?.GetImGuiTextureIcon((uint)iconId)?.ImGuiHandle;
    }

    private static int InnerFrameWidth => (ScorePanelWidth * 2) - 21;
    private static int InnerFrameHeight => ScorePanelHeight - 85;
    public List<Tf2MvpMember> PartyList { get; set; }
    public Team PlayerTeam { get; set; }
    public Team WinningTeam { get; set; }


    public override void PostDraw()
    {
        ImGui.PopStyleVar();
        base.PostDraw();
    }
}
