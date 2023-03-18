using System;
using Dalamud.Game;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.System.Scheduler.Base;
using Tf2CriticalHitsPlugin.Countdown.Configuration;
using Tf2CriticalHitsPlugin.Countdown.Game;
using Tf2CriticalHitsPlugin.Countdown.Status;
using Tf2CriticalHitsPlugin.SeFunctions;

namespace Tf2CriticalHitsPlugin.Countdown;

public class CountdownModule: IDisposable
{
    private readonly State state;
    private readonly CountdownConfigZero config;
    private readonly CountdownHook countdownHook;

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
        countdownHook.Update();
    }

    private static void OnStartCountingDown(object? sender, EventArgs args)
    {
        if (sender is null) return;
        var state = (State)sender;
        SoundEngine.PlaySound("C:\\Users\\Bernardo\\Desktop\\mvm_start_wave.wav", 0.1f, "cd");

    }

    private static void OnStopCountingDown(object? sender, EventArgs e)
    {
        if (sender is null) return;
        var state = (State)sender;
        if (state.countdownCancelled)
        {
            SoundEngine.StopSound("cd");
            SoundEngine.PlaySound("C:\\Users\\Bernardo\\Downloads\\record-scratch-freesounds-luffy-3536.wav", 0.1f);
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
