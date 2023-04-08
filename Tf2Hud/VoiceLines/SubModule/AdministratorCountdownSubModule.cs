using System;
using System.Collections.Generic;
using Dalamud.Game;
using KamiLib;
using Tf2Hud.Common.Audio;
using Tf2Hud.Common.Configuration;
using Tf2Hud.VoiceLines.Game;
using Tf2Hud.VoiceLines.Model;

namespace Tf2Hud.VoiceLines.SubModule;

public class AdministratorCountdownSubModule: VoiceLinesSubModule
{
    private readonly CountdownHook countdownHook;
    private readonly HashSet<int> countdownPlayedFor = new();
    private readonly CountdownState countdownState;

    
    public AdministratorCountdownSubModule(
        ConfigZero.GeneralConfigZero generalConfig, ConfigZero.VoiceLinesConfigZero voiceLinesConfig) : base(
        generalConfig, voiceLinesConfig, voiceLinesConfig.AdministratorCountdown)
    {
        countdownState = CountdownState.Instance();
        countdownHook = new CountdownHook(countdownState, CriticalCommonLib.Service.Condition);
        CriticalCommonLib.Service.Framework.Update += OnUpdate;
        
        countdownState.StartCountingDown += OnStartCountingDown;
        countdownState.StopCountingDown += OnStopCountingDown;

    }
    public override void Dispose()
    {
        countdownState.StopCountingDown -= OnStopCountingDown;
        countdownState.StartCountingDown -= OnStartCountingDown;
        
        CriticalCommonLib.Service.Framework.Update -= OnUpdate;
        countdownHook.Dispose();
    }
    
        private void OnUpdate(Framework framework)
    {
        countdownHook.Update();
        UpdateCountdown();
    }
    
    private void UpdateCountdown()
    {
        if (!countdownState.CountingDown) return;
        AdministratorCountdownOngoing();
    }

    private void AdministratorCountdownOngoing()
    {
        if (!ShouldPlay) return;
        var ceil = (int)Math.Ceiling(countdownState.CountDownValue);
        if (countdownPlayedFor.Contains(ceil))
        {
            if (countdownState.CountDownValue < 0.01f && !countdownPlayedFor.Contains(0))
            {
                countdownPlayedFor.Add(0);
                var sound = Tf2Sound.Instance.RandomGoSound;
                SoundEngine.PlaySoundAsync(sound, GeneralConfig.ApplySfxVolume, GeneralConfig.Volume.Value);
                VoiceLinesConfig.AdministratorCountdown.Heard.Value = true;
                KamiCommon.SaveConfiguration();
            }
        }
        else
        {
            countdownPlayedFor.Add(ceil);
            var sound = Tf2Sound.Instance.RandomCountdownSound(ceil);
            SoundEngine.PlaySoundAsync(sound, GeneralConfig.ApplySfxVolume, GeneralConfig.Volume.Value);
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
        if (!ShouldPlay) return;
        if (!countdownState.countdownCancelled && !countdownPlayedFor.Contains(0))
        {
            countdownPlayedFor.Add(0);
            var sound = Tf2Sound.Instance.RandomGoSound;
            SoundEngine.PlaySoundAsync(sound, GeneralConfig.ApplySfxVolume, GeneralConfig.Volume.Value);
            VoiceLinesConfig.AdministratorCountdown.Heard.Value = true;
            KamiCommon.SaveConfiguration();
        }
    }

}
