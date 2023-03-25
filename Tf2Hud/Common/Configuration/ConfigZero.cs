using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ImGuiNET;
using KamiLib.Configuration;
using Newtonsoft.Json;
using Tf2Hud.Common.Util;
using Tf2Hud.Configuration;
using Tf2Hud.Tf2Hud.Model;
using Tf2Hud.Tf2Hud.Windows;

namespace Tf2Hud.Common.Configuration;

public class ConfigZero : BaseConfiguration
{
    public ConfigZero()
    {
        Version = 0;
    }

    public class ClassConfigZero
    {
        public readonly Setting<bool> UsePerJob = new(false);
        public Setting<Tf2Class> GlobalClass { get; set; } = new(Enum.GetValues<Tf2Class>().Random());
        public readonly IDictionary<uint, Setting<Tf2Class>> ClassPerJob = InitializeClassDictionary();

        private static IDictionary<uint,Setting<Tf2Class>> InitializeClassDictionary()
        {
            return Constants.CombatJobs.Select(
                                c => new KeyValuePair<uint, Tf2Class>(
                                    c.Key, Tf2ClassHelper.GetTf2ClassFromXivCombatClass(c.Value)))
                            .ToDictionary(it => it.Key, it => new Setting<Tf2Class>(it.Value));
        }

        public int Version { get; set; } = 0;
    }

    public class TimerConfigZero : ModuleConfiguration
    {
        public override float GetPositionXDefault() => (ImGui.GetMainViewport().Size.X / 2) - 110;

        public override float GetPositionYDefault() => 50;
    }

    public class WinPanelConfigZero : ModuleConfiguration
    {
        public WinPanelConfigZero()
        {
            Version = 0;
        }

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

    public readonly ClassConfigZero Class = new();
    public readonly TimerConfigZero Timer = new();
    public readonly WinPanelConfigZero WinPanel = new();

    public void Save()
    {
        PluginVersion = PluginVersion.Current;
        File.WriteAllText(Service.PluginInterface.ConfigFile.FullName,
                          JsonConvert.SerializeObject(this, Formatting.Indented));
    }
}
