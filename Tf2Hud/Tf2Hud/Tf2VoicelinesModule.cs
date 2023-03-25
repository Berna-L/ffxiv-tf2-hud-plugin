using System;
using System.Linq;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using Lumina.Excel.GeneratedSheets;
using Tf2Hud.Common.Configuration;
using Tf2Hud.Tf2Hud.Audio;

namespace Tf2Hud.Tf2Hud;

public class Tf2VoiceLinesModule: IDisposable
{
    private readonly ConfigZero configZero;

    public Tf2VoiceLinesModule(ConfigZero configZero)
    {
        this.configZero = configZero;
        Service.DutyState.DutyStarted += OnStart;

    }

    private void OnStart(object? sender, ushort e)
    {
        if (IsHighEndDuty())
        {
            SoundEngine.PlaySound(Tf2Sound.Instance.RandomMannUpSound, configZero.ApplySfxVolume, configZero.Volume.Value);
        }
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


    public void Dispose()
    {
        Service.DutyState.DutyStarted -= OnStart;
    }
}
