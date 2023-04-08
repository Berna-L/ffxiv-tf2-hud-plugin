using System.Linq;
using KamiLib;
using Lumina.Excel.GeneratedSheets;
using Tf2Hud.Common.Audio;
using Tf2Hud.Common.Configuration;

namespace Tf2Hud.VoiceLines.SubModule;

public class MannUpSubModule: VoiceLinesSubModule
{
    public MannUpSubModule(
        ConfigZero.GeneralConfigZero generalConfig, ConfigZero.VoiceLinesConfigZero voiceLinesConfig) : base(
        generalConfig, voiceLinesConfig, voiceLinesConfig.MannUp)
    {
        Service.DutyState.DutyStarted += OnStart;
    }

    public override void Dispose()
    {
        Service.DutyState.DutyStarted -= OnStart;
    }

    private void OnStart(object? sender, ushort e)
    {
        if (IsHighEndDuty && ShouldPlay)
        {
            SoundEngine.PlaySoundAsync(Tf2Sound.Instance.RandomMannUpSound, GeneralConfig.ApplySfxVolume,
                                       GeneralConfig.Volume.Value);
            WasHeard();
        }

    }

    private static bool IsHighEndDuty
    {
        get
        {
            var contentDirector = Service.ContentDirector;
            if (contentDirector is null) return false;
            return CriticalCommonLib.Service.Data.GetExcelSheet<ContentFinderCondition>()?
                       .FirstOrDefault(cfc => cfc.Content == contentDirector.Value.Director.ContentId)?
                       .HighEndDuty ?? false;
        }
    }
}
