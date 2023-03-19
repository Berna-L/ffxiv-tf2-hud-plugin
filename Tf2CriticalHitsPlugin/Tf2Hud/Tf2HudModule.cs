using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using KamiLib;
using Tf2CriticalHitsPlugin.Common.Windows;
using Tf2CriticalHitsPlugin.Tf2Hud.Windows;

namespace Tf2CriticalHitsPlugin.Tf2Hud;

public class Tf2HudModule: IDisposable
{
    private readonly Tf2WinPanel tf2WinPanel;

    private int bluScore = 0;
    private int redScore = 0;
    private List<DeadEnemy> deadEnemies = new();

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
        tf2WinPanel = new Tf2WinPanel();
    }

    private void OnUpdate(Framework? framework)
    {
        foreach (var deadEnemy in Service.ObjectTable.Where(ot => ot.SubKind == (int)BattleNpcSubKind.Enemy)
                                     .Where(ot => ot.IsDead)
                                     .Select(ot => new DeadEnemy(ot))
                                     .Where(de => !deadEnemies.Contains(de)))
        {
            deadEnemies.Add(deadEnemy);
        }
    }

    private void OnStart(object? sender, ushort e)
    {
        bluScore = 0;
        redScore = 0;
        deadEnemies.Clear();
    }

    private void OnComplete(object? sender, ushort e)
    {
        bluScore += 1;
        OnUpdate(null);
        tf2WinPanel.Show(bluScore, redScore, deadEnemies.Last().name.TextValue, Tf2Window.TeamColor.Blu);
    }

    private void OnWipe(object? sender, ushort e)
    {
        PluginLog.LogDebug("================================");
        foreach (var gameObject in Service.ObjectTable.Where(ot => ot.SubKind == (int)BattleNpcSubKind.Enemy).Where(ot => !ot.IsDead).ToList())
        {
                PluginLog.LogDebug($"{gameObject.Name.TextValue} | {gameObject.TargetObject?.Name ?? ""}");
        }
        PluginLog.LogDebug("================================");
        var enemy = Service.ObjectTable.Where(ot => ot.SubKind == (int)BattleNpcSubKind.Enemy)
                           .Where(ot => !ot.IsDead)
                                .Select(ot => ot.Name.TextValue)
                                .GroupBy(name => name)
                                .OrderByDescending(g => g.Count())
                                .Take(1).SingleOrDefault()?.Key ?? "an anonymous enemy";

        redScore += 1;
        tf2WinPanel.Show(bluScore, redScore, enemy, Tf2Window.TeamColor.Red);
    }

    public void Dispose()
    {
        Service.DutyState.DutyStarted -= OnStart;
        Service.DutyState.DutyCompleted -= OnComplete;
        Service.DutyState.DutyWiped -= OnWipe;
        Service.Framework.Update -= OnUpdate;

    }
}
