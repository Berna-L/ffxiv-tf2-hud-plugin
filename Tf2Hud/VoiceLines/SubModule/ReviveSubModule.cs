using Tf2Hud.Common.Audio;
using Tf2Hud.Common.Configuration;
using Tf2Hud.Common.Service;

namespace Tf2Hud.VoiceLines.SubModule;

public class ReviveSubModule: VoiceLinesSubModule
{

    
    public ReviveSubModule(ConfigZero.GeneralConfigZero generalConfig, ConfigZero.VoiceLinesConfigZero voiceLinesConfig)
        : base(generalConfig, voiceLinesConfig, voiceLinesConfig.Revive)
    {
        Services.PlayerStatus.OnRevive += OnRevive;
    }

    public override void Dispose()
    {
        Services.PlayerStatus.OnRevive -= OnRevive;
    }

    private void OnRevive(object? sender, PlayerStatusService.ReviveType e)
    {
        var player = CriticalCommonLib.Service.ClientState.LocalPlayer;
        if (player is null) return;
        var currentClass = GeneralConfig.Class.CurrentClass(player);
        if (currentClass is null) return;
        SoundEngine.PlaySoundAsync(Tf2Sound.Instance.RandomReviveSound(currentClass.Value), GeneralConfig);
        WasHeard();
    }
}
