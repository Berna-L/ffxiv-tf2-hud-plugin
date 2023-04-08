using System;
using KamiLib;
using Tf2Hud.Common.Configuration;

namespace Tf2Hud.VoiceLines.SubModule;

public abstract class VoiceLinesSubModule: IDisposable
{
    protected readonly ConfigZero.GeneralConfigZero GeneralConfig;
    protected readonly ConfigZero.VoiceLinesConfigZero VoiceLinesConfig;
    protected readonly ConfigZero.VoiceLinesConfigZero.VoiceLineTrigger Trigger;

    protected VoiceLinesSubModule(ConfigZero.GeneralConfigZero generalConfig, ConfigZero.VoiceLinesConfigZero voiceLinesConfig, ConfigZero.VoiceLinesConfigZero.VoiceLineTrigger trigger)
    {
        this.GeneralConfig = generalConfig;
        this.VoiceLinesConfig = voiceLinesConfig;
        Trigger = trigger;
    }

    protected bool ShouldPlay => VoiceLinesConfig.Enabled && Trigger.Enabled;

    public abstract void Dispose();

    protected void WasHeard()
    {
        Trigger.Heard.Value = true;
        KamiCommon.SaveConfiguration();
    }
}
