using System;
using System.Linq;
using CriticalCommonLib;
using Dalamud.Game;
using Tf2Hud.Common.Audio;
using Tf2Hud.Common.Configuration;

namespace Tf2Hud.VoiceLines.SubModule;

public class FlawlessVictorySubModule: VoiceLinesSubModule
{
    private bool wasThereAnyDeath;
    private long? completeTimestamp;

    public FlawlessVictorySubModule(
        ConfigZero.GeneralConfigZero generalConfig, ConfigZero.VoiceLinesConfigZero voiceLinesConfig) : base(
        generalConfig, voiceLinesConfig, voiceLinesConfig.FlawlessVictory)
    {
        Services.DutyState.DutyStarted += OnDutyStarted;
        Services.DutyState.DutyCompleted += OnDutyCompleted;

        Service.Framework.Update += OnUpdate;
    }

    public override void Dispose()
    {
        Service.Framework.Update -= OnUpdate;

        Services.DutyState.DutyStarted -= OnDutyStarted;
        Services.DutyState.DutyCompleted -= OnDutyCompleted;
    }

    private void OnUpdate(Framework framework)
    {
        if (wasThereAnyDeath) return;
        foreach (var partyMember in Services.PartyList.Select(p => p.GameObject).Where(g => g is not null))
        {
            if (partyMember!.IsDead)
            {
                wasThereAnyDeath = true;
                return;
            }
        }

        if (completeTimestamp is not null && DateTimeOffset.Now.ToUnixTimeMilliseconds() - completeTimestamp > 2500)
        {
            completeTimestamp = null;
            if (ShouldPlay)
            {
                SoundEngine.PlaySoundAsync(Tf2Sound.Instance.RandomFlawlessVictorySound, GeneralConfig);
                WasHeard();
            }
        }
    }

    private void OnDutyStarted(object? sender, ushort e)
    {
        wasThereAnyDeath = false;
        completeTimestamp = null;
    }

    private void OnDutyCompleted(object? sender, ushort e)
    {
        if (!wasThereAnyDeath)
        {
            completeTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
        else
        {
            completeTimestamp = null;
        }
    }
}
