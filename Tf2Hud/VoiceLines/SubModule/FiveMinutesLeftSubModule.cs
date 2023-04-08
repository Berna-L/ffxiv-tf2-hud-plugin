using Dalamud.Game;
using KamiLib;
using Tf2Hud.Common.Audio;
using Tf2Hud.Common.Configuration;

namespace Tf2Hud.VoiceLines.SubModule;

public class FiveMinutesLeftSubModule: VoiceLinesSubModule
{
    private bool playedForFiveMinutesLeft;

    public FiveMinutesLeftSubModule(
        ConfigZero.GeneralConfigZero generalConfig, ConfigZero.VoiceLinesConfigZero voiceLinesConfig) : base(
        generalConfig, voiceLinesConfig, voiceLinesConfig.FiveMinutesLeft)
    {
        CriticalCommonLib.Service.Framework.Update += OnUpdate;
    }


    public override void Dispose()
    {
        CriticalCommonLib.Service.Framework.Update -= OnUpdate;
    }

    private void OnUpdate(Framework framework)
    {
        if (ShouldPlay &&
            Service.DutyState.IsDutyStarted &&
            !playedForFiveMinutesLeft &&
            (Service.ContentDirector?.ContentTimeLeft ?? int.MaxValue) < 5 * 60)
        {
            playedForFiveMinutesLeft = true;
            SoundEngine.PlaySoundAsync(Tf2Sound.Instance.FiveMinutesLeftSound, GeneralConfig.ApplySfxVolume,
                                       GeneralConfig.Volume.Value);
            WasHeard();
        }
    }
}
