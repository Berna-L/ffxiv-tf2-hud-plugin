using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using KamiLib;
using Lumina.Excel.GeneratedSheets;
using Tf2Hud.Common.Audio;
using Tf2Hud.Common.Configuration;
using Tf2Hud.VoiceLines.Game;
using Tf2Hud.VoiceLines.Model;
using static Tf2Hud.Common.Configuration.ConfigZero.VoiceLinesConfigZero;

namespace Tf2Hud.VoiceLines;

public class Tf2VoiceLinesModule : IDisposable
{
    private static IDictionary<int, Func<Audio?>> SoundsPerSecond = new Dictionary<int, Func<Audio?>>();
    private readonly CountdownHook countdownHook;
    private readonly HashSet<int> CountdownPlayedFor = new();
    private readonly CountdownState CountdownState;
    private readonly ConfigZero.GeneralConfigZero generalConfig;
    private readonly ConfigZero.VoiceLinesConfigZero voiceLinesConfig;

    public Tf2VoiceLinesModule(
        ConfigZero.GeneralConfigZero generalConfig, ConfigZero.VoiceLinesConfigZero voiceLinesConfig)
    {
        this.generalConfig = generalConfig;
        this.voiceLinesConfig = voiceLinesConfig;
        CountdownState = CountdownState.Instance();
        countdownHook = new CountdownHook(CountdownState, Service.Condition);

        Service.Framework.Update += OnUpdate;

        Service.DutyState.DutyStarted += OnStart;

        CountdownState.StartCountingDown += OnStartCountingDown;
        CountdownState.StopCountingDown += OnStopCountingDown;
    }


    public void Dispose()
    {
        CountdownState.StopCountingDown -= OnStopCountingDown;
        CountdownState.StartCountingDown -= OnStartCountingDown;
        Service.DutyState.DutyStarted -= OnStart;
        Service.Framework.Update -= OnUpdate;
        countdownHook.Dispose();
    }

    private void OnUpdate(Framework framework)
    {
        countdownHook.Update();
        UpdateCountdown();
    }

    private void UpdateCountdown()
    {
        if (!CountdownState.CountingDown) return;
        AdministratorCountdownOngoing();
    }

    private void AdministratorCountdownOngoing()
    {
        if (!voiceLinesConfig.AdministratorCountdown.Enabled) return;
        var ceil = (int)Math.Ceiling(CountdownState.CountDownValue);
        if (CountdownPlayedFor.Contains(ceil))
        {
            if (CountdownState.CountDownValue < 0.01f && !CountdownPlayedFor.Contains(0))
            {
                CountdownPlayedFor.Add(0);
                var sound = Tf2Sound.Instance.RandomGoSound;
                if (sound is null) return;
                SoundEngine.PlaySound(sound, generalConfig.ApplySfxVolume, generalConfig.Volume.Value);
                voiceLinesConfig.AdministratorCountdown.Heard.Value = true;
                KamiCommon.SaveConfiguration();
            }
        }
        else
        {
            CountdownPlayedFor.Add(ceil);
            var sound = Tf2Sound.Instance.RandomCountdownSound(ceil);
            if (sound is null) return;
            SoundEngine.PlaySound(sound, generalConfig.ApplySfxVolume, generalConfig.Volume.Value);
        }
    }

    private void OnStartCountingDown(object? sender, EventArgs e)
    {
        CountdownPlayedFor.Clear();
        UpdateCountdown();
    }

    private void OnStopCountingDown(object? sender, EventArgs e)
    {
        AdministratorCountdownZero();
    }

    private void AdministratorCountdownZero()
    {
        if (!voiceLinesConfig.MannUp.Enabled) return;
        if (!CountdownState.countdownCancelled && !CountdownPlayedFor.Contains(0))
        {
            CountdownPlayedFor.Add(0);
            var sound = Tf2Sound.Instance.RandomGoSound;
            if (sound is null) return;
            SoundEngine.PlaySound(sound, generalConfig.ApplySfxVolume, generalConfig.Volume.Value);
            voiceLinesConfig.AdministratorCountdown.Heard.Value = true;
            KamiCommon.SaveConfiguration();
        }
    }

    private void OnStart(object? sender, ushort e)
    {
        if (IsHighEndDuty() && ShouldPlay(voiceLinesConfig.MannUp))
        {
            SoundEngine.PlaySound(Tf2Sound.Instance.RandomMannUpSound, generalConfig.ApplySfxVolume,
                                  generalConfig.Volume.Value);
            voiceLinesConfig.MannUp.Heard.Value = true;
            KamiCommon.SaveConfiguration();
        }
    }

    private bool ShouldPlay(VoiceLineTrigger trigger)
    {
        return voiceLinesConfig.Enabled && trigger.Enabled;
    }

    private static unsafe bool IsHighEndDuty()
    {
        return Service.DataManager.GetExcelSheet<ContentFinderCondition>()?
                   .FirstOrDefault(cfc => cfc.Content == EventFramework
                                                      .Instance()
                                                  ->GetInstanceContentDirector()
                                              ->ContentDirector.Director.ContentId)?
                   .HighEndDuty ?? false;
    }
}
