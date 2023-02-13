using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Configuration;
using Dalamud.Logging;
using Dalamud.Plugin;
using KamiLib.Configuration;
using Lumina.Excel.GeneratedSheets;
using static Tf2CriticalHitsPlugin.Constants;

namespace Tf2CriticalHitsPlugin.Configuration;

[Serializable]
public class ConfigOne : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public SortedDictionary<uint, JobConfig> JobConfigurations { get; set; } = new();

    public ConfigOne()
    {
        foreach (var (key, _) in CombatJobs)
        {
            JobConfigurations[key] = new JobConfig(key);
        }
    }

    public class JobConfig
    {
        public uint ClassJobId;
        public ConfigModule DirectCriticalDamage { get; set; }
        public ConfigModule CriticalDamage { get; set; }
        public ConfigModule CriticalHeal { get; set; }
        public ConfigModule DirectDamage { get; set; }

        
        public JobConfig(uint classJobId)
        {
            ClassJobId = classJobId;
            DirectCriticalDamage = new ConfigModule(classJobId, (ushort)ModuleType.DirectCriticalDamage);
            CriticalDamage = new ConfigModule(classJobId, (ushort)ModuleType.CriticalDamage);
            CriticalHeal = new ConfigModule(classJobId, (ushort)ModuleType.CriticalHeal);
            DirectDamage = new ConfigModule(classJobId, (ushort)ModuleType.DirectDamage);
        }

        public ClassJob GetClassJob() => CombatJobs[ClassJobId];

        public IEnumerator<ConfigModule> GetEnumerator()
        {
            return new[] { DirectCriticalDamage, CriticalDamage, CriticalHeal, DirectDamage }.ToList().GetEnumerator();
        }
    }

    public class ConfigModule
    {

        public ConfigModule(uint classJobId, ushort moduleTypeId)
        {
            this.ClassJobId = new Setting<uint>(classJobId);
            this.ModuleTypeId = new Setting<ushort>(moduleTypeId);
            this.ModuleDefaults = ModuleConstants.GetConstantsFromType((ModuleType) moduleTypeId);
            this.Text = new Setting<string>(ModuleDefaults.DefaultText);
            TextColor = ModuleDefaults.FlyTextParameters.ColorKey;
            TextGlowColor = ModuleDefaults.FlyTextParameters.GlowColorKey;
            TextItalics = ModuleDefaults.FlyTextParameters.Italics;
        }

        public string GetId()
        {
            PluginLog.LogDebug($"{ClassJobId}{ModuleTypeId}");
            return $"{ClassJobId}{ModuleTypeId}";
        }

        [NonSerialized]
        public ModuleConstants ModuleDefaults;

        public Setting<uint> ClassJobId { get; set; }
        public Setting<ushort> ModuleTypeId { get; set; }
        public Setting<bool> PlaySound { get; set; } = new(true);
        public Setting<bool> SoundForActionsOnly { get; set; } = new(false);
        public Setting<string> FilePath { get; set; } = new(string.Empty);
        public Setting<int> Volume { get; set; } = new(12);
        public Setting<bool> ShowText { get; set; } = new(true);
        public Setting<string> Text { get; set; }
        public Setting<ushort> TextColor { get; set; }
        public Setting<ushort> TextGlowColor { get; set; }
        public Setting<bool> TextItalics { get; set; }
        
    }

    // the below exist just to make saving less cumbersome
    [NonSerialized]
    private DalamudPluginInterface? pluginInterface;


    public void Initialize(DalamudPluginInterface pluginInterface)
    {
        this.pluginInterface = pluginInterface;
    }

    public void Save()
    {
        this.pluginInterface!.SavePluginConfig(this);
    }
}
