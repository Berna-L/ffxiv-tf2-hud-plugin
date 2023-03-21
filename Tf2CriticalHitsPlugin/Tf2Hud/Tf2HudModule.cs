using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Logging;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using KamiLib;
using Sledge.Formats.Packages;
using Tf2CriticalHitsPlugin.Common.Windows;
using Tf2CriticalHitsPlugin.Tf2Hud.Windows;

namespace Tf2CriticalHitsPlugin.Tf2Hud;

public class Tf2HudModule: IDisposable
{
    private readonly Tf2WinPanel tf2WinPanel;

    private byte[]? victorySound;
    private byte[]? failSound;
    private byte[]? scoredSound;
    private int bluScore;
    private int redScore;
    private List<DeadEnemy> deadEnemies = new();
    private uint lastDutyTerritory;

    private class DeadEnemy
    {
        public uint id { get; }
        public SeString name { get; }

        public DeadEnemy(GameObject gameObject)
        {
            id = gameObject.ObjectId;
            name = gameObject.Name;
        }
    }
    
        
    public Tf2HudModule()
    {
        Service.DutyState.DutyStarted += OnStart;
        Service.DutyState.DutyCompleted += OnComplete;
        Service.DutyState.DutyWiped += OnWipe;
        Service.Framework.Update += OnUpdate;

        KamiCommon.WindowManager.AddWindow(new Tf2BluScore());
        KamiCommon.WindowManager.AddWindow(new Tf2RedScore());
        KamiCommon.WindowManager.AddWindow(new Tf2MvpList());
        KamiCommon.WindowManager.AddWindow(new Tf2TimerWindow());

        ReadSoundFilesFromTf2();

        tf2WinPanel = new Tf2WinPanel(scoredSound);
    }

    private void ReadSoundFilesFromTf2()
    {
        const string tf2VpkPath = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Team Fortress 2\\tf\\tf2_sound_misc_dir.vpk";
        if (!Path.Exists(tf2VpkPath))
        {
            return;
        }
        using var package = new VpkPackage(tf2VpkPath);
        
        var victory = package.Entries.First(e => e.Name == "your_team_won.wav");
        using var victoryStream = package.Open(victory);
        victorySound = new byte[victoryStream.Length];
        victoryStream.Read(victorySound, 0, victorySound.Length);
        
        var fail = package.Entries.First(e => e.Name == "your_team_lost.wav");
        using var failStream = package.Open(fail);
        failSound = new byte[failStream.Length];
        failStream.Read(failSound, 0, failSound.Length);
        
        var scored = package.Entries.First(e => e.Path == "sound/ui/scored.wav");
        using var scoredStream = package.Open(scored);
        scoredSound = new byte[scoredStream.Length];
        PluginLog.Debug(scoredSound.Length.ToString());
        scoredStream.Read(scoredSound, 0, scoredSound.Length);
        
        package.Dispose();
        scoredStream.Dispose();
        failStream.Dispose();
        victoryStream.Dispose();
    }

    private void OnUpdate(Framework? framework)
    {
        UpdateTimer();
        foreach (var deadEnemy in Service.ObjectTable.Where(ot => ot.SubKind == (int)BattleNpcSubKind.Enemy)
                                     .Where(ot => ot.IsDead)
                                     .Select(ot => new DeadEnemy(ot))
                                     .Where(de => !deadEnemies.Contains(de)))
        {
            deadEnemies.Add(deadEnemy);
        }
    }

    private static unsafe void UpdateTimer()
    {
        var contentDirector = EventFramework.Instance()->GetInstanceContentDirector();
        if (KamiCommon.WindowManager.GetWindowOfType<Tf2TimerWindow>() is { } window)
        {
            window.timeRemaining = contentDirector is not null ? (long) Math.Round(contentDirector->ContentDirector.ContentTimeLeft) : null;
            window.IsOpen = contentDirector is not null;
        }
    }

    private void OnStart(object? sender, ushort e)
    {
        if (Service.ClientState.TerritoryType != lastDutyTerritory)
        {
            lastDutyTerritory = Service.ClientState.TerritoryType;
            bluScore = 0;
            redScore = 0;
            Tf2WinPanel.ClearScores();
        }
        deadEnemies.Clear();
    }

    private void OnComplete(object? sender, ushort e)
    {
        bluScore += 1;
        OnUpdate(null);
        tf2WinPanel.Show(bluScore, redScore, deadEnemies.Last().name.TextValue, Tf2Window.TeamColor.Blu);
        if (victorySound is null) return;
        SoundEngine.PlaySound(victorySound, true, 50);
    }

    private void OnWipe(object? sender, ushort e)
    {
        var enemy = Service.ObjectTable.Where(ot => ot.SubKind == (int)BattleNpcSubKind.Enemy)
                           .Where(ot => !ot.IsDead)
                           .Select(ot => ot.Name.TextValue)
                           .Where(name => !name.IsNullOrWhitespace())
                           .GroupBy(name => name)
                           .OrderByDescending(g => g.Count())
                           .Take(1).SingleOrDefault()?.Key?.Trim();
        enemy = enemy.IsNullOrWhitespace() ? "an anonymous enemy" : enemy;
        redScore += 1;
        tf2WinPanel.Show(bluScore, redScore, enemy, Tf2Window.TeamColor.Red);
        if (failSound is null) return;
        SoundEngine.PlaySound(failSound, true, 50);
    }

    public void Dispose()
    {
        Service.DutyState.DutyStarted -= OnStart;
        Service.DutyState.DutyCompleted -= OnComplete;
        Service.DutyState.DutyWiped -= OnWipe;
        Service.Framework.Update -= OnUpdate;

    }
}
