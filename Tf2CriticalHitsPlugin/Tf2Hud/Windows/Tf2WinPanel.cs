using System;
using System.Linq;
using System.Numerics;
using Dalamud.Game;
using KamiLib;
using Tf2CriticalHitsPlugin.Common.Windows;

namespace Tf2CriticalHitsPlugin.Tf2Hud.Windows;

public class Tf2WinPanel: IDisposable
{

    private long timeOpened;

    public bool IsOpen
    {
        get => KamiCommon.WindowManager.GetWindowOfType<Tf2BluScore>()?.IsOpen ?? false;
        set
        {
            GetBluScoreWindow().IsOpen = value;
            GetRedScoreWindow().IsOpen = value;
            GetMvpList().IsOpen = value;
        }
    }

    public Tf2WinPanel()
    {
        Service.Framework.Update += OnUpdate;
    }

    private void OnUpdate(Framework framework)
    {
        if (IsOpen && DateTimeOffset.UtcNow.ToUnixTimeSeconds() - timeOpened > 10)
        {
            IsOpen = false;
        }
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

    public void Show(int bluScore, int redScore, string lastEnemy, Vector4 victorColor)
    {
        GetBluScoreWindow().Score = bluScore;
        GetRedScoreWindow().Score = redScore;
        GetMvpList().BackgroundColor = victorColor;
        GetMvpList().LastEnemy = lastEnemy;
        IsOpen = true;
        timeOpened = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public void Dispose()
    {
        Service.Framework.Update -= OnUpdate;
    }
}
