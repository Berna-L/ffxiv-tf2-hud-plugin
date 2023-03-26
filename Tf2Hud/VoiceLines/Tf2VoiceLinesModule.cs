using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using KamiLib;
using Lumina.Excel.GeneratedSheets;
using Tf2Hud.Common.Audio;
using Tf2Hud.Common.Configuration;
using Tf2Hud.Common.Windows;
using Tf2Hud.VoiceLines.Game;
using Tf2Hud.VoiceLines.Model;
using static Tf2Hud.Common.Configuration.ConfigZero.VoiceLinesConfigZero;

namespace Tf2Hud.VoiceLines;

public class Tf2VoiceLinesModule : IDisposable
{
    private readonly ConfigZero.GeneralConfigZero generalConfig;
    private readonly ConfigZero.VoiceLinesConfigZero voiceLinesConfig;

    private readonly CountdownHook countdownHook;
    private readonly HashSet<int> countdownPlayedFor = new();
    private readonly CountdownState countdownState;

    private bool playedForFiveMinutesLeft = false;
    
    public Tf2VoiceLinesModule(
        ConfigZero.GeneralConfigZero generalConfig, ConfigZero.VoiceLinesConfigZero voiceLinesConfig)
    {
        this.generalConfig = generalConfig;
        this.voiceLinesConfig = voiceLinesConfig;
        countdownState = CountdownState.Instance();
        countdownHook = new CountdownHook(countdownState, Service.Condition);

        Service.Framework.Update += OnUpdate;

        Service.DutyState.DutyStarted += OnStart;

        countdownState.StartCountingDown += OnStartCountingDown;
        countdownState.StopCountingDown += OnStopCountingDown;
    }


    public void Dispose()
    {
        countdownState.StopCountingDown -= OnStopCountingDown;
        countdownState.StartCountingDown -= OnStartCountingDown;
        
        Service.DutyState.DutyStarted -= OnStart;
        
        Service.Framework.Update -= OnUpdate;
        countdownHook.Dispose();
    }

    private void OnUpdate(Framework framework)
    {
        countdownHook.Update();
        UpdateCountdown();
        UpdateTimeLeft();
    }

    private unsafe void UpdateTimeLeft()
    {
        if (!playedForFiveMinutesLeft &&
            EventFramework.Instance()->GetInstanceContentDirector()->ContentDirector.ContentTimeLeft < 5 * 60)
        {
            playedForFiveMinutesLeft = true;
            SoundEngine.PlaySound(Tf2Sound.Instance.FiveMinutesLeftSound, generalConfig.ApplySfxVolume, generalConfig.Volume.Value);
            voiceLinesConfig.FiveMinutesLeft.Heard.Value = true;
            KamiCommon.SaveConfiguration();
        }
    }

    private void UpdateCountdown()
    {
        if (!countdownState.CountingDown) return;
        AdministratorCountdownOngoing();
    }

    private void AdministratorCountdownOngoing()
    {
        if (!voiceLinesConfig.AdministratorCountdown.Enabled) return;
        var ceil = (int)Math.Ceiling(countdownState.CountDownValue);
        if (countdownPlayedFor.Contains(ceil))
        {
            if (countdownState.CountDownValue < 0.01f && !countdownPlayedFor.Contains(0))
            {
                countdownPlayedFor.Add(0);
                var sound = Tf2Sound.Instance.RandomGoSound;
                if (sound is null) return;
                SoundEngine.PlaySound(sound, generalConfig.ApplySfxVolume, generalConfig.Volume.Value);
                voiceLinesConfig.AdministratorCountdown.Heard.Value = true;
                KamiCommon.SaveConfiguration();
            }
        }
        else
        {
            countdownPlayedFor.Add(ceil);
            var sound = Tf2Sound.Instance.RandomCountdownSound(ceil);
            if (sound is null) return;
            SoundEngine.PlaySound(sound, generalConfig.ApplySfxVolume, generalConfig.Volume.Value);
        }
    }

    private void OnStartCountingDown(object? sender, EventArgs e)
    {
        countdownPlayedFor.Clear();
        UpdateCountdown();
    }

    private void OnStopCountingDown(object? sender, EventArgs e)
    {
        AdministratorCountdownZero();
    }

    private void AdministratorCountdownZero()
    {
        if (!voiceLinesConfig.MannUp.Enabled) return;
        if (!countdownState.countdownCancelled && !countdownPlayedFor.Contains(0))
        {
            countdownPlayedFor.Add(0);
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
