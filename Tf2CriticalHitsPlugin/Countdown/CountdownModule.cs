using System;
using System.Linq;
using Dalamud.Game;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using KamiLib;
using Tf2CriticalHitsPlugin.Common.Windows;
using Tf2CriticalHitsPlugin.Countdown.Configuration;
using Tf2CriticalHitsPlugin.Countdown.Game;
using Tf2CriticalHitsPlugin.Countdown.Status;

namespace Tf2CriticalHitsPlugin.Countdown;

public class CountdownModule : IDisposable
{
    private readonly State state;
    private readonly CountdownConfigZero config;
    private readonly CountdownHook countdownHook;
    private bool playedForCurrentCountdown = false;

    public CountdownModule(State state, CountdownConfigZero config)
    {
        this.countdownHook = new CountdownHook(state, Service.Condition);
        this.state = state;
        this.config = config;
        state.StartCountingDown += OnStartCountingDown;
        state.StopCountingDown += OnStopCountingDown;
        Service.Framework.Update += OnUpdate;
    }

    private void OnUpdate(Framework framework)
    {
        test();
        countdownHook.Update();
        var module = config.modules
                           .Where(m => m.Enabled)
                           .Where(m => m.DelayPlay)
                           .Where(m => m.ValidForCountdown(state.StartingValue))
                           .FirstOrDefault(m => m.ValidForTerritory(Service.ClientState.TerritoryType));
        if (module is null) return;
        if (!state.CountingDown || state.CountDownValue > module.DelayUntilCountdownHits.Value ||
            playedForCurrentCountdown) return;
        SoundEngine.PlaySound(module.FilePath.Value, module.ApplySfxVolume, module.Volume.Value, $"countdown|{module.Id}");
        playedForCurrentCountdown = true;
    }

    private static unsafe void test()
    {
        var contentDirector = EventFramework.Instance()->GetInstanceContentDirector();
        // if (contentDirector is null)
        // {
        if (KamiCommon.WindowManager.GetWindowOfType<Tf2TimerWindow>() is { } window)
        {
                window.timeRemaining = contentDirector is not null ? (long) Math.Round(contentDirector->ContentDirector.ContentTimeLeft) : null;
                window.IsOpen = contentDirector is not null;
        }
    }

    private void OnStartCountingDown(object? sender, EventArgs args)
    {
        playedForCurrentCountdown = false;
        if (sender is null) return;
        var state = (State)sender;
        var module = config.modules
                           .Where(m => m.Enabled)
                           .Where(m => !m.DelayPlay)
                           .Where(m => m.ValidForCountdown(state.StartingValue))
                           .FirstOrDefault(m => m.ValidForTerritory(Service.ClientState.TerritoryType));

        if (module is null) return;
        SoundEngine.PlaySound(module.FilePath.Value, module.ApplySfxVolume, module.Volume.Value,
                              $"countdown|{module.Id}");
        playedForCurrentCountdown = true;
    }

    private void OnStopCountingDown(object? sender, EventArgs e)
    {
        if (sender is null) return;
        var state = (State)sender;
        if (state.countdownCancelled)
        {
            var moduleToStop = config.modules
                                     .FirstOrDefault(m => SoundEngine.IsPlaying($"countdown|{m.Id}"));
            if (moduleToStop is null) return;

            SoundEngine.StopSound($"countdown|{moduleToStop.Id}");
            SoundEngine.PlaySound(moduleToStop.InterruptedFilePath.Value, moduleToStop.InterruptedApplySfxVolume,
                                  moduleToStop.InterruptedVolume.Value, $"countdownstop");
        }
    }

    public void Dispose()
    {
        Service.Framework.Update -= OnUpdate;
        state.StartCountingDown -= OnStartCountingDown;
        state.StopCountingDown -= OnStopCountingDown;
        countdownHook.Dispose();
    }
}
