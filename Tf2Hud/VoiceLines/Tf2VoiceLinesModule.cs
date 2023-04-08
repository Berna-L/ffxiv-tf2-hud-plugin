using System;
using System.Collections.Generic;
using Tf2Hud.Common.Configuration;
using Tf2Hud.VoiceLines.SubModule;

namespace Tf2Hud.VoiceLines;

public sealed class Tf2VoiceLinesModule : IDisposable
{
    private readonly ISet<VoiceLinesSubModule> subModules = new HashSet<VoiceLinesSubModule>();
    
    public Tf2VoiceLinesModule(
        ConfigZero.GeneralConfigZero generalConfig, ConfigZero.VoiceLinesConfigZero voiceLinesConfig)
    {
        subModules.Add(new AugmentationTokenSubModule(generalConfig, voiceLinesConfig));
        subModules.Add(new FiveMinutesLeftSubModule(generalConfig, voiceLinesConfig));
        subModules.Add(new MannUpSubModule(generalConfig, voiceLinesConfig));
        subModules.Add(new AdministratorCountdownSubModule(generalConfig, voiceLinesConfig));
        subModules.Add(new ReviveSubModule(generalConfig, voiceLinesConfig));
    }


    public void Dispose()
    {
        foreach (var subModule in subModules)
        {
            subModule.Dispose();
        }
    }
    
}
