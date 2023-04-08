using Dalamud.Game;
using Tf2Hud.Common.Audio;
using Tf2Hud.Common.Configuration;

namespace Tf2Hud.VoiceLines.SubModule;

public class LastOneAliveSubModule : VoiceLinesSubModule
{

    public LastOneAliveSubModule(
        ConfigZero.GeneralConfigZero generalConfig, ConfigZero.VoiceLinesConfigZero voiceLinesConfig) : base(
        generalConfig, voiceLinesConfig, voiceLinesConfig.LastOneAlive)
    {
        Services.PlayerStatus.OnLastOneAlive += OnLastOneAlive;
    }


    public override void Dispose()
    {
        Services.PlayerStatus.OnLastOneAlive -= OnLastOneAlive;
    }

    private void OnLastOneAlive()
    {
        if (ShouldPlay)
        {
            SoundEngine.PlaySoundAsync(Tf2Sound.Instance.RandomLastOneAliveSound, GeneralConfig);
            WasHeard();
        }
    }
}
