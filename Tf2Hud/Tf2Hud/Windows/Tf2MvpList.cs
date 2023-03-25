using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface.Raii;
using Dalamud.Utility;
using ImGuiNET;
using KamiLib.Drawing;
using Pilz.Dalamud.Icons;
using Tf2Hud.Common.Configuration;
using Tf2Hud.Common.Windows;

namespace Tf2Hud.Tf2Hud.Windows;

public class Tf2MvpList : Tf2Window
{
    private const string PointsThisRound = "(everyone is an MVP :)";
    private readonly JobIconSets jobIconSets;
    private readonly GameFontHandle playerNameFont;
    private static readonly Vector2 ClassJobIconSize = new(32, 32);
    private static readonly Vector2 DefaultPosition = new((ImGui.GetMainViewport().Size.X / 2) - ScorePanelWidth, ImGui.GetMainViewport().Size.Y - 500 + ScorePanelHeight);
    private ImRaii.Style frameRounding;
    private ImRaii.Color windowBg; 


    public Tf2MvpList() : base("##Tf2MvpList", Team.Red)
    {
        Size = new Vector2(ScorePanelWidth * 2, MvpListHeight);
        jobIconSets = new JobIconSets();
        playerNameFont = Service.PluginInterface.UiBuilder.GetGameFontHandle(new GameFontStyle(GameFontFamily.Axis, 18));
        Position = DefaultPosition;
        PartyList = new List<Tf2MvpMember>();
        PlayerTeam = Team.Red;
        WinningTeam = Team.Red;
    }

    public string? LastEnemy { get; set; }

    public override void PreDraw()
    {
        base.PreDraw();
        Service.Log($"Tf2MvpList - PostDraw");
        frameRounding = ImRaii.PushStyle(ImGuiStyleVar.FrameRounding, 0f);
        windowBg = ImRaii.PushColor(ImGuiCol.WindowBg, WinningTeam.BgColor);
    }

    public override void Draw()
    {

        Service.Log($"Tf2MvpList - Starting to draw");
        var teamWinMessage = $"{WinningTeam.Name} TEAM WINS!";
        ImGui.PushFont(Tf2SecondaryFont);
        ImGui.SetCursorPosX((ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(teamWinMessage).X) / 2);
        ImGuiHelper.TextShadow(teamWinMessage);
        ImGui.PopFont();

        Service.Log($"Tf2MvpList - Adding what happened");
        var enemyName = LastEnemy.IsNullOrWhitespace() ? "an anonymous enemy" : LastEnemy;
        var winConditionMessage = WinningTeam == PlayerTeam
                                      ? $"{WinningTeam.Name} defeated {enemyName} before the time ran out"
                                      : $"{WinningTeam.Name}, led by {enemyName}, wiped {WinningTeam.Enemy.Name}";
        
        ImGui.SetCursorPosX((ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(winConditionMessage).X) / 2);
        ImGui.Text(winConditionMessage);
        Service.Log($"Tf2MvpList - Creating player names' area");
        ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.85f);
        ImGui.PushStyleColor(ImGuiCol.FrameBg, Colors.Black.ToU32());
        ImGui.BeginChildFrame(12313, new Vector2(InnerFrameWidth, InnerFrameHeight));
        ImGui.PopStyleColor();
        ImGui.PopStyleVar();
        ImGui.Text($"{PlayerTeam.Name} team MVPs:");
        ImGui.SameLine();
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(PointsThisRound).X);
        ImGui.Text(PointsThisRound);
        ImGui.GetWindowDrawList().AddLine(ImGui.GetCursorScreenPos() + new Vector2(0, 0),
                                          ImGui.GetCursorScreenPos() + new Vector2(InnerFrameWidth, 0), Colors.White.ToU32());
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 10);
        var middlePosX = ImGui.GetCursorPosX() + (ImGui.GetContentRegionAvail().X / 2);
        ImGui.PushFont(playerNameFont.ImFont);
        for (var i = 0; i < PartyList.Count; i++)
        {
            if (i >= PartyList.Count) break;
            Service.Log($"Tf2MvpList - Adding player {i}");
            var leftPartyMember = PartyList[i];
            ImGui.Image(GetClassJobIcon(leftPartyMember.ClassJobId)!.Value, ClassJobIconSize);
            ImGui.SameLine();
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 6);
            ImGui.TextColored(WinningTeam.TextColor, leftPartyMember.Name.ToDesiredFormat(NameDisplay));
            if (PartyList.Count > 4 && PartyList.Count > i + 1)
            {
                Service.Log($"Tf2MvpList - Adding player {i + 1}");
                var rightPartyMember = PartyList[i + 1];
                i++;
                ImGui.SameLine();
                ImGui.SetCursorPosX(middlePosX);
                var classJobIcon = GetClassJobIcon(rightPartyMember.ClassJobId);
                if (classJobIcon is not null)
                {
                    ImGui.Image(classJobIcon!.Value, ClassJobIconSize);
                    ImGui.SameLine();
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 6);
                    ImGui.TextColored(WinningTeam.TextColor, rightPartyMember.Name.ToDesiredFormat(NameDisplay));
                }
            }
        }
        ImGui.PopFont();
        ImGui.EndChildFrame();
        
    }

    public override void PostDraw()
    {
        Service.Log($"Tf2MvpList - PostDraw");
        windowBg.Dispose();
        frameRounding.Dispose();
        base.PostDraw();
    }

    private nint? GetClassJobIcon(uint id)
    {
        var iconId = jobIconSets.GetJobIcon(JobIconSetName.Framed, id);
        return Service.DataManager?.GetImGuiTextureIcon((uint)iconId)?.ImGuiHandle;
    }


    private static int InnerFrameWidth => (ScorePanelWidth * 2) - 21;

    private static int InnerFrameHeight => MvpListHeight - 85;

    public List<Tf2MvpMember> PartyList { get; set; }

    public NameDisplayKind NameDisplay { get; set; }

    public Team PlayerTeam { get; set; }

    public Team WinningTeam { get; set; }
}

static class Extension
{
    public static string ToDesiredFormat(this string s, NameDisplayKind nameDisplay)
    {
        var fullName = s.Split(' ');
        var forenameAbbrev = $"{fullName[0][0]}.";
        var surnameAbbrev = $"{fullName[1][0]}.";
        return nameDisplay switch
        {
            NameDisplayKind.FullName => s,
            NameDisplayKind.ForenameAbbreviated => forenameAbbrev + " " + fullName[1],
            NameDisplayKind.SurnameAbbreviated => fullName[0] + " " + surnameAbbrev,
            NameDisplayKind.Initials => forenameAbbrev + " " + surnameAbbrev,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
