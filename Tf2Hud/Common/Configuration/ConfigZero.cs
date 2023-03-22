using System;
using System.IO;
using KamiLib.Configuration;
using Newtonsoft.Json;
using Tf2Hud.Configuration;

namespace Tf2Hud.Common.Configuration;

public class ConfigZero : BaseConfiguration
{
    public ConfigZero()
    {
        Version = 0;
    }

    public Setting<string>? Tf2InstallPath { get; set; } = null;
    public Setting<TeamPreferenceKind> TeamPreference { get; set; } = new(TeamPreferenceKind.Random);
    public Setting<ScoreBehaviorKind> ScoreBehavior { get; set; } = new(ScoreBehaviorKind.ResetIfDutyChanged);
    
    
    public void Save()
    {
        PluginVersion = PluginVersion.Current;
        File.WriteAllText(Service.PluginInterface.ConfigFile.FullName,
                          JsonConvert.SerializeObject(this, Formatting.Indented));
    }
}

public enum TeamPreferenceKind
{
    Blu,
    Red,
    Random
}

public enum ScoreBehaviorKind
{
    ResetEveryInstance,
    ResetIfDutyChanged,
    ResetUponClosingGame
}
