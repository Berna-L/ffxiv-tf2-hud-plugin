using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game;
using Dalamud.Game.ClientState.Party;
using KamiLib;
using Tf2Hud.Common.Windows;

namespace Tf2Hud.Tf2Hud.Windows;

public class Tf2WinPanel : IDisposable
{
    private readonly byte[]? scoredSound;
    private int newPlayerTeamScore;
    private int newEnemyTeamScore;

    private long timeOpened;
    private bool waitingForNewScore;

    public Tf2WinPanel(byte[]? scoredSound)
    {
        this.scoredSound = scoredSound;
        Service.Framework.Update += OnUpdate;
    }

    public bool IsOpen
    {
        get => KamiCommon.WindowManager.GetWindowOfType<Tf2BluScoreWindow>()?.IsOpen ?? false;
        set
        {
            GetPlayerTeamScoreWindow().IsOpen = value;
            GetEnemyTeamScoreWindow().IsOpen = value;
            GetMvpList().IsOpen = value;
        }
    }

    public Team PlayerTeam { get; set; }

    public void Dispose()
    {
        Service.Framework.Update -= OnUpdate;
    }

    private void OnUpdate(Framework framework)
    {
        var openedFor = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - timeOpened;
        if (IsOpen && openedFor > 2 && waitingForNewScore)
        {
            GetPlayerTeamScoreWindow().Score = newPlayerTeamScore;
            GetEnemyTeamScoreWindow().Score = newEnemyTeamScore;
            if (scoredSound is not null) SoundEngine.PlaySound(scoredSound, true, 50, 22050, 1);

            waitingForNewScore = false;
        }

        if (IsOpen && openedFor > 10) IsOpen = false;
    }

    private static Tf2MvpList GetMvpList()
    {
        return KamiCommon.WindowManager.GetWindowOfType<Tf2MvpList>()!;
    }

    private Tf2TeamScoreWindow GetPlayerTeamScoreWindow()
    {
        return PlayerTeam.IsBlu ? KamiCommon.WindowManager.GetWindowOfType<Tf2BluScoreWindow>()!
                   : KamiCommon.WindowManager.GetWindowOfType<Tf2RedScoreWindow>()!;
    }

    private Tf2TeamScoreWindow GetEnemyTeamScoreWindow()
    {
        return PlayerTeam.Enemy.IsBlu ? KamiCommon.WindowManager.GetWindowOfType<Tf2BluScoreWindow>()!
                   : KamiCommon.WindowManager.GetWindowOfType<Tf2RedScoreWindow>()!;
    }

    public void Show(int playerTeamScore, int enemyTeamScore, List<Tf2MvpMember> partyList, string lastEnemy, Team winningTeam)
    {
        waitingForNewScore = true;
        timeOpened = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        GetMvpList().WinningTeam = winningTeam;
        GetMvpList().PlayerTeam = PlayerTeam;
        GetMvpList().PartyList = partyList;
        GetMvpList().LastEnemy = lastEnemy;
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
