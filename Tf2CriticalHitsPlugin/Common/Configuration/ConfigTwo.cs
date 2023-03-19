using System;
using System.IO;
using Newtonsoft.Json;
using Tf2CriticalHitsPlugin.Configuration;
using Tf2CriticalHitsPlugin.Countdown.Configuration;
using Tf2CriticalHitsPlugin.CriticalHits.Configuration;

namespace Tf2CriticalHitsPlugin.Common.Configuration;

public class ConfigTwo: BaseConfiguration
{
    public CriticalHitsConfigOne criticalHits;
    public CountdownConfigZero countdownJams;
    
    public ConfigTwo()
    {
        Version = 2;
        criticalHits = new CriticalHitsConfigOne();
        countdownJams = new CountdownConfigZero();
    }

    public void Save()
    {
        this.PluginVersion = PluginVersion.Current;
        File.WriteAllText(Service.PluginInterface.ConfigFile.FullName,
                          JsonConvert.SerializeObject(this, Formatting.Indented));
    }
}
