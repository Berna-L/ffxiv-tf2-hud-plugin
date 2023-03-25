using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface.Raii;
using Dalamud.Utility;
using ImGuiNET;
using ImGuiScene;
using KamiLib.Caching;
using KamiLib.Drawing;
using Pilz.Dalamud.Icons;
using Tf2Hud.Common.Configuration;
using Tf2Hud.Common.Windows;
using Tf2Hud.Tf2Hud.Model;

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


    public Tf2MvpList() : base("##Tf2MvpList", Tf2Team.Red)
    {
        Size = new Vector2(ScorePanelWidth * 2, MvpListHeight);
        jobIconSets = new JobIconSets();
        playerNameFont = Service.PluginInterface.UiBuilder.GetGameFontHandle(new GameFontStyle(GameFontFamily.Axis, 18));
        Position = DefaultPosition;
        PartyList = new List<Tf2MvpMember>();
        PlayerTeam = Tf2Team.Red;
        WinningTeam = Tf2Team.Red;
    }

    public string? LastEnemy { get; set; }

    public override void PreDraw()
    {
        base.PreDraw();
        frameRounding = ImRaii.PushStyle(ImGuiStyleVar.FrameRounding, 0f);
        windowBg = ImRaii.PushColor(ImGuiCol.WindowBg, WinningTeam.BgColor);
    }

    public override void Draw()
    {
        var teamWinMessage = $"{WinningTeam.Name} TEAM WINS!";
        using (ImRaii.PushFont(Tf2SecondaryFont))
        {
            ImGui.SetCursorPosX((ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(teamWinMessage).X) / 2);
            ImGuiHelper.TextShadow(teamWinMessage);
        }

        var enemyName = LastEnemy.IsNullOrWhitespace() ? "an anonymous enemy" : LastEnemy;
        var winConditionMessage = WinningTeam == PlayerTeam
                                      ? $"{WinningTeam.Name} defeated {enemyName} before the time ran out"
                                      : $"{WinningTeam.Name}, led by {enemyName}, wiped {WinningTeam.Enemy.Name}";
        
        ImGui.SetCursorPosX((ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(winConditionMessage).X) / 2);
        ImGui.Text(winConditionMessage);

        using var alpha = ImRaii.PushStyle(ImGuiStyleVar.Alpha, 0.85f);
        using var frameBg = ImRaii.PushColor(ImGuiCol.FrameBg, Colors.Black.ToU32());
        ImGui.BeginChildFrame((uint) Math.Abs(Random.Shared.Next()), new Vector2(InnerFrameWidth, InnerFrameHeight));
        ImGui.Text($"{PlayerTeam.Name} team MVPs:");
        ImGui.SameLine();
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(PointsThisRound).X);
        ImGui.Text(PointsThisRound);
        ImGui.GetWindowDrawList().AddLine(ImGui.GetCursorScreenPos() + new Vector2(0, 0),
                                          ImGui.GetCursorScreenPos() + new Vector2(InnerFrameWidth, 0), Colors.White.ToU32());
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 10);
        var middlePosX = ImGui.GetCursorPosX() + (ImGui.GetContentRegionAvail().X / 2);
        
        using var playerFont = ImRaii.PushFont(playerNameFont.ImFont);
        for (var i = 0; i < PartyList.Count; i++)
        {
            if (i >= PartyList.Count) break;
            var leftPartyMember = PartyList[i];
            DrawPlayer(leftPartyMember);
            if (PartyList.Count > 4 && PartyList.Count > i + 1)
            {
                var rightPartyMember = PartyList[i + 1];
                i++;
                ImGui.SetCursorPosX(middlePosX);
                ImGui.SameLine();
                DrawPlayer(rightPartyMember);
            }
        }
        ImGui.EndChildFrame();
    }

    private void DrawPlayer(Tf2MvpMember member)
    {
        var classJobIcon = GetClassJobIcon(member.ClassJobId);
        if (classJobIcon is not null)
        {
            ImGui.Image(classJobIcon.ImGuiHandle, ClassJobIconSize);
            ImGui.SameLine();
        }
        else
        {
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ClassJobIconSize.X);
        }
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 6);
        ImGui.TextColored(WinningTeam.TextColor, member.Name.ToDesiredFormat(NameDisplay));
    }

    public override void PostDraw()
    {
        windowBg.Dispose();
        frameRounding.Dispose();
        base.PostDraw();
    }

    private TextureWrap? GetClassJobIcon(uint id)
    {
        var iconId = jobIconSets.GetJobIcon(JobIconSetName.Framed, id);
        return IconCache.Instance.GetIcon((uint)iconId);
    }


    private static int InnerFrameWidth => (ScorePanelWidth * 2) - 21;

    private static int InnerFrameHeight => MvpListHeight - 85;

    public List<Tf2MvpMember> PartyList { get; set; }

    public NameDisplayKind NameDisplay { get; set; }

    public Tf2Team PlayerTeam { get; set; }

    public Tf2Team WinningTeam { get; set; }
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
