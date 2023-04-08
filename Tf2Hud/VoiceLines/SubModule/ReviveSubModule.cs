using System.Linq;
using Dalamud.Game;
using Tf2Hud.Common.Audio;
using Tf2Hud.Common.Configuration;

namespace Tf2Hud.VoiceLines.SubModule;

public class ReviveSubModule: VoiceLinesSubModule
{
    private const uint Weakness = 43;
    private const uint BrinkOfDeath = 44;
    
    private bool playerWasDead;
    
    public ReviveSubModule(ConfigZero.GeneralConfigZero generalConfig, ConfigZero.VoiceLinesConfigZero voiceLinesConfig)
        : base(generalConfig, voiceLinesConfig, voiceLinesConfig.Revive)
    {
        CriticalCommonLib.Service.Framework.Update += OnUpdate;
    }

    public override void Dispose()
    {
        CriticalCommonLib.Service.Framework.Update -= OnUpdate;
    }

    private void OnUpdate(Framework framework)
    {
        var player = CriticalCommonLib.Service.ClientState.LocalPlayer;
        if (player is null) return;
        var currentClass = GeneralConfig.Class.CurrentClass(player);
        if (currentClass is null) return;
        if (player.IsDead)
        {
            playerWasDead = true;
            return;
        }

        if (playerWasDead)
        {
            if (player.StatusList.Any(s => s.StatusId is Weakness or BrinkOfDeath))
            {
                SoundEngine.PlaySoundAsync(Tf2Sound.Instance.RandomReviveSound(currentClass.Value), GeneralConfig);
                WasHeard();
            }
        }
        playerWasDead = false;
    }
}
