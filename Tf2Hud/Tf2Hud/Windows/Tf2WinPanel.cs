﻿using System;
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
    private int newBluScore;
    private int newRedScore;

    private long timeOpened;
    private bool waitingForNewScore;

    public Tf2WinPanel(byte[]? scoredSound)
    {
        this.scoredSound = scoredSound;
        Service.Framework.Update += OnUpdate;
    }

    public static bool IsOpen
    {
        get => KamiCommon.WindowManager.GetWindowOfType<Tf2BluScore>()?.IsOpen ?? false;
        set
        {
            GetBluScoreWindow().IsOpen = value;
            GetRedScoreWindow().IsOpen = value;
            GetMvpList().IsOpen = value;
        }
    }

    public void Dispose()
    {
        Service.Framework.Update -= OnUpdate;
    }

    private void OnUpdate(Framework framework)
    {
        var openedFor = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - timeOpened;
        if (IsOpen && openedFor > 2 && waitingForNewScore)
        {
            GetBluScoreWindow().Score = newBluScore;
            GetRedScoreWindow().Score = newRedScore;
            if (scoredSound is not null) SoundEngine.PlaySound(scoredSound, true, 50, 22050, 1);

            waitingForNewScore = false;
        }

        if (IsOpen && openedFor > 10) IsOpen = false;
    }

    private static Tf2MvpList GetMvpList()
    {
        return KamiCommon.WindowManager.GetWindowOfType<Tf2MvpList>()!;
    }

    private static Tf2RedScore GetRedScoreWindow()
    {
        return KamiCommon.WindowManager.GetWindowOfType<Tf2RedScore>()!;
    }

    private static Tf2BluScore GetBluScoreWindow()
    {
        return KamiCommon.WindowManager.GetWindowOfType<Tf2BluScore>()!;
    }

    public void Show(int bluScore, int redScore, List<PartyMember> partyList, string lastEnemy, Vector4 victorColor)
    {
        IsOpen = true;
        waitingForNewScore = true;
        timeOpened = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        GetMvpList().BackgroundColor = victorColor;
        GetMvpList().PartyList = partyList;
        GetMvpList().LastEnemy = lastEnemy;
        newBluScore = bluScore;
        newRedScore = redScore;
    }

    public static void ClearScores()
    {
        GetBluScoreWindow().Score = 0;
        GetRedScoreWindow().Score = 0;
    }
}
