using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game;
using KamiLib;
using KamiLib.Configuration;
using Tf2Hud.Common.Audio;
using Tf2Hud.Common.Configuration;
using Tf2Hud.Common.Model;

namespace Tf2Hud.Tf2Hud.Windows;

public class Tf2WinPanel : IDisposable
{
    private readonly ConfigZero configZero;
    private int enemyTeamScoreToSet;
    private int playerTeamScoreToSet;

    private long timeOpened;
    private bool waitingForNewScore;


    public Tf2WinPanel(ConfigZero configZero, Tf2Team playerTeam, List<Tf2MvpMember> initialPartyList)
    {
        this.configZero = configZero;
        PlayerTeam = playerTeam;
        MvpListWindow.PlayerTeam = PlayerTeam;
        MvpListWindow.WinningTeam = PlayerTeam;
        MvpListWindow.PartyList = initialPartyList;
        Service.Framework.Update += OnUpdate;
    }

    private static Tf2BluScoreWindow BluScoreWindow => KamiCommon.WindowManager.GetWindowOfType<Tf2BluScoreWindow>()!;
    private static Tf2RedScoreWindow RedScoreWindow => KamiCommon.WindowManager.GetWindowOfType<Tf2RedScoreWindow>()!;
    private static Tf2MvpList MvpListWindow => KamiCommon.WindowManager.GetWindowOfType<Tf2MvpList>()!;

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

    public Tf2Team PlayerTeam { get; set; }
    public Setting<bool> RepositionMode { get; set; } = new(false);

    public void Dispose()
    {
        Service.Framework.Update -= OnUpdate;
    }

    private void OnUpdate(Framework framework)
    {
        var openedFor = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - timeOpened;

        if (IsOpen)
        {
            BluScoreWindow.Position = configZero.WinPanel.GetPosition();
            RedScoreWindow.Position = configZero.WinPanel.GetPosition() + new Vector2(Tf2Window.ScorePanelWidth, 0);
            MvpListWindow.Position = configZero.WinPanel.GetPosition() + new Vector2(0, Tf2Window.ScorePanelHeight);
        }

        if (IsOpen && openedFor > 2 && waitingForNewScore)
        {
            GetPlayerTeamScoreWindow().Score = playerTeamScoreToSet;
            GetEnemyTeamScoreWindow().Score = enemyTeamScoreToSet;
            SoundEngine.PlaySound(Tf2Sound.Instance.ScoredSound, configZero.ApplySfxVolume, configZero.Volume.Value);
            waitingForNewScore = false;
        }

        if (RepositionMode) IsOpen = true;

        if (IsOpen && openedFor > configZero.WinPanel.TimeToClose.Value)
            if (!configZero.WinPanel.RepositionMode)
                IsOpen = false;
    }


    private Tf2TeamScoreWindow GetPlayerTeamScoreWindow()
    {
        return PlayerTeam.IsBlu ? BluScoreWindow : RedScoreWindow!;
    }


    private Tf2TeamScoreWindow GetEnemyTeamScoreWindow()
    {
        return PlayerTeam.Enemy.IsBlu ? BluScoreWindow : RedScoreWindow!;
    }

    public void Show(
        int oldPlayerTeamScore, int oldEnemyTeamScore, int newPlayerTeamScore, int newEnemyTeamScore,
        List<Tf2MvpMember> partyList, string? lastEnemy, Tf2Team winningTeam)
    {
        waitingForNewScore = true;
        timeOpened = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        MvpListWindow.WinningTeam = winningTeam;
        MvpListWindow.PlayerTeam = PlayerTeam;
        MvpListWindow.PartyList = partyList;
        MvpListWindow.NameDisplay = configZero.WinPanel.NameDisplay.Value;
        MvpListWindow.LastEnemy = lastEnemy;
        GetPlayerTeamScoreWindow().Score = oldPlayerTeamScore;
        GetEnemyTeamScoreWindow().Score = oldEnemyTeamScore;
        playerTeamScoreToSet = newPlayerTeamScore;
        enemyTeamScoreToSet = newEnemyTeamScore;
        IsOpen = true;
    }

    public void ClearScores()
    {
        GetPlayerTeamScoreWindow().Score = 0;
        GetEnemyTeamScoreWindow().Score = 0;
    }
}
