using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game;
using ImGuiNET;
using KamiLib;
using KamiLib.Configuration;
using Tf2Hud.Common.Configuration;
using Tf2Hud.Common.Windows;

namespace Tf2Hud.Tf2Hud.Windows;

public class Tf2WinPanel : IDisposable
{
    private readonly ConfigZero configZero;
    private readonly byte[]? scoredSound;
    private int newPlayerTeamScore;
    private int newEnemyTeamScore;

    private long timeOpened;
    private bool waitingForNewScore;
    private static Tf2BluScoreWindow BluScoreWindow => KamiCommon.WindowManager.GetWindowOfType<Tf2BluScoreWindow>()!;
    private static Tf2RedScoreWindow RedScoreWindow => KamiCommon.WindowManager.GetWindowOfType<Tf2RedScoreWindow>()!;
    private static Tf2MvpList MvpListWindow => KamiCommon.WindowManager.GetWindowOfType<Tf2MvpList>()!;


    public Tf2WinPanel(ConfigZero configZero, Team playerTeam, List<Tf2MvpMember> initialPartyList, byte[]? scoredSound)
    {
        this.configZero = configZero;
        this.scoredSound = scoredSound;
        PlayerTeam = playerTeam;
        MvpListWindow.PlayerTeam = PlayerTeam;
        MvpListWindow.WinningTeam = PlayerTeam;
        MvpListWindow.PartyList = initialPartyList;
        Service.Framework.Update += OnUpdate;
    }

    public bool IsOpen
    {
        get => KamiCommon.WindowManager.GetWindowOfType<Tf2BluScoreWindow>()?.IsOpen ?? false;
        set
        {
            var actualValue = value && configZero.WinPanel.Enabled;
            GetPlayerTeamScoreWindow().IsOpen = actualValue;
            GetEnemyTeamScoreWindow().IsOpen = actualValue;
            MvpListWindow.IsOpen = actualValue;
        }
    }

    public Team PlayerTeam { get; set; }
    public Setting<bool> RepositionMode { get; set; }

    public void Dispose()
    {
        Service.Framework.Update -= OnUpdate;
    }

    private void OnUpdate(Framework framework)
    {
        BluScoreWindow.Position = configZero.WinPanel.GetPosition();
        RedScoreWindow.Position = configZero.WinPanel.GetPosition() + new Vector2(Tf2Window.ScorePanelWidth, 0);
        MvpListWindow.Position = configZero.WinPanel.GetPosition() + new Vector2(0, Tf2Window.ScorePanelHeight);
        var openedFor = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - timeOpened;
        if (IsOpen && openedFor > 2 && waitingForNewScore)
        {
            GetPlayerTeamScoreWindow().Score = newPlayerTeamScore;
            GetEnemyTeamScoreWindow().Score = newEnemyTeamScore;
            if (scoredSound is not null) SoundEngine.PlaySound(scoredSound, true, 50, 22050, 1);

            waitingForNewScore = false;
        }

        if (RepositionMode)
        {
            IsOpen = true;
        }

        if (IsOpen && openedFor > configZero.WinPanel.TimeToClose.Value && !configZero.WinPanel.RepositionMode) IsOpen = false;
    }



    private Tf2TeamScoreWindow GetPlayerTeamScoreWindow()
    {
        return PlayerTeam.IsBlu ? BluScoreWindow
                   : RedScoreWindow!;
    }


    private Tf2TeamScoreWindow GetEnemyTeamScoreWindow()
    {
        return PlayerTeam.Enemy.IsBlu ? BluScoreWindow
                   : RedScoreWindow!;
    }

    public void Show(int playerTeamScore, int enemyTeamScore, List<Tf2MvpMember> partyList, string? lastEnemy, Team winningTeam)
    {
        waitingForNewScore = true;
        timeOpened = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        MvpListWindow.WinningTeam = winningTeam;
        MvpListWindow.PlayerTeam = PlayerTeam;
        MvpListWindow.PartyList = partyList;
        MvpListWindow.LastEnemy = lastEnemy;
        GetPlayerTeamScoreWindow().Score = newPlayerTeamScore;
        GetEnemyTeamScoreWindow().Score = newEnemyTeamScore;
        newPlayerTeamScore = playerTeamScore;
        newEnemyTeamScore = enemyTeamScore;
        IsOpen = true;
    }

    public void ClearScores()
    {
        GetPlayerTeamScoreWindow().Score = 0;
        GetEnemyTeamScoreWindow().Score = 0;
    }
}
