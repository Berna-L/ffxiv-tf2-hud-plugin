using System;
using System.IO;
using Dalamud.Configuration;
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

    public class TimerConfigZero : ModuleConfiguration
    {

    }

    public class WinPanelConfigZero : ModuleConfiguration
    {
        public Setting<ScoreBehaviorKind> ScoreBehavior { get; set; } = new(ScoreBehaviorKind.ResetIfDutyChanged);
        public Setting<int> TimeToClose { get; set; } = new(10);

    }
    
    public Setting<string> Tf2InstallPath { get; set; } = new(string.Empty);

    [NonSerialized]
    public bool Tf2InstallPathAutoDetected;
    
    public Setting<TeamPreferenceKind> TeamPreference { get; set; } = new(TeamPreferenceKind.Random);

    public TimerConfigZero Timer = new();
    public WinPanelConfigZero WinPanel = new();

    public void Save()
    {
        PluginVersion = PluginVersion.Current;
        File.WriteAllText(Service.PluginInterface.ConfigFile.FullName,
                          JsonConvert.SerializeObject(this, Formatting.Indented));
    }
}
