using System;
using System.IO;
using ImGuiNET;
using KamiLib.Configuration;
using Newtonsoft.Json;
using Tf2Hud.Configuration;
using Tf2Hud.Tf2Hud.Windows;

namespace Tf2Hud.Common.Configuration;

public class ConfigZero : BaseConfiguration
{
    public ConfigZero()
    {
        Version = 0;
    }

    public class TimerConfigZero : ModuleConfiguration
    {
        public override float GetPositionXDefault() => (ImGui.GetMainViewport().Size.X / 2) - 110;

        public override float GetPositionYDefault() => 50;
    }

    public class WinPanelConfigZero : ModuleConfiguration
    {
        public Setting<ScoreBehaviorKind> ScoreBehavior { get; set; } = new(ScoreBehaviorKind.ResetIfDutyChanged);
        public Setting<int> TimeToClose { get; set; } = new(10);

        public Setting<NameDisplayKind> NameDisplay { get; set; } = new(NameDisplayKind.FullName);
        
        public override float GetPositionXDefault() => (ImGui.GetMainViewport().Size.X / 2) - Tf2Window.ScorePanelWidth;

        public override float GetPositionYDefault() => ImGui.GetMainViewport().Size.Y - 500;
    }
    
    public Setting<string> Tf2InstallPath { get; set; } = new(string.Empty);

    [NonSerialized]
    public bool Tf2InstallPathAutoDetected;
    
    public Setting<TeamPreferenceKind> TeamPreference { get; set; } = new(TeamPreferenceKind.Random);

    public Setting<int> Volume { get; set; } = new(50);
    public Setting<bool> ApplySfxVolume { get; set; } = new(true);
    
    public TimerConfigZero Timer = new();
    public WinPanelConfigZero WinPanel = new();

    public void Save()
    {
        PluginVersion = PluginVersion.Current;
        File.WriteAllText(Service.PluginInterface.ConfigFile.FullName,
                          JsonConvert.SerializeObject(this, Formatting.Indented));
    }
}
