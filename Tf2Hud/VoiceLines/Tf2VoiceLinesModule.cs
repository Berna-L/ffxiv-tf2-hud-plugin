using System;
using System.Linq;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using Lumina.Excel.GeneratedSheets;
using Tf2Hud.Common.Audio;
using Tf2Hud.Common.Configuration;
using static Tf2Hud.Common.Configuration.ConfigZero.VoiceLinesConfigZero;
using static Tf2Hud.VoiceLines.Model.VoiceLineTriggerType;

namespace Tf2Hud.VoiceLines;

public class Tf2VoiceLinesModule : IDisposable
{
    private readonly ConfigZero.GeneralConfigZero generalConfig;
    private readonly ConfigZero.VoiceLinesConfigZero voiceLinesConfig;

    public Tf2VoiceLinesModule(ConfigZero.GeneralConfigZero generalConfig, ConfigZero.VoiceLinesConfigZero voiceLinesConfig)
    {
        this.generalConfig = generalConfig;
        this.voiceLinesConfig = voiceLinesConfig;
        Service.DutyState.DutyStarted += OnStart;
    }


    public void Dispose()
    {
        Service.DutyState.DutyStarted -= OnStart;
    }

    private void OnStart(object? sender, ushort e)
    {
        if (IsHighEndDuty() && ShouldPlay(voiceLinesConfig.Triggers[MannUpWhenStartingHighEndDuty]))
            SoundEngine.PlaySound(Tf2Sound.Instance.RandomMannUpSound, generalConfig.ApplySfxVolume,
                                  generalConfig.Volume.Value);
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
