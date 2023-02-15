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
            JobConfigurations[key] = JobConfig.Create(key);
        }
    }

    public class JobConfig
    {
        public Setting<uint> ClassJobId { get; init; } = new(255);
        public ConfigModule DirectCriticalDamage { get; init; } = new();
        public ConfigModule CriticalDamage { get; init; } = new();
        public ConfigModule CriticalHeal { get; init; } = new();
        public ConfigModule DirectDamage { get; init; } = new();


        public static JobConfig Create(uint classJobId)
        {
            return new JobConfig
            {
                ClassJobId = new Setting<uint>(classJobId),
                DirectCriticalDamage = ConfigModule.Create(classJobId, ModuleType.DirectCriticalDamage),
                CriticalDamage = ConfigModule.Create(classJobId, ModuleType.CriticalDamage),
                CriticalHeal = ConfigModule.Create(classJobId, ModuleType.CriticalHeal),
                DirectDamage = ConfigModule.Create(classJobId, ModuleType.DirectDamage)
            };
        }
        

        public ClassJob GetClassJob() => CombatJobs[ClassJobId.Value];

        public IEnumerator<ConfigModule> GetEnumerator()
        {
            return new[] { DirectCriticalDamage, CriticalDamage, CriticalHeal, DirectDamage }.ToList().GetEnumerator();
        }

        public void CopySettingsFrom(JobConfig jobConfig)
        {
            DirectCriticalDamage.CopySettingsFrom(jobConfig.DirectCriticalDamage);
            CriticalDamage.CopySettingsFrom(jobConfig.CriticalDamage);
            CriticalHeal.CopySettingsFrom(jobConfig.CriticalHeal);
            DirectDamage.CopySettingsFrom(jobConfig.DirectDamage);
        }
    }

    public class ConfigModule
    {

        public static ConfigModule Create(uint classJobId, ModuleType moduleType)
        {
            var configModule = new ConfigModule
            {
                ClassJobId = new Setting<uint>(classJobId),
                ModuleType = new Setting<ModuleType>(moduleType),
            };
            var moduleDefaults = ModuleDefaults.GetDefaultsFromType(moduleType);
            configModule.Text = new Setting<string>(moduleDefaults.DefaultText);
            configModule.TextColor = new Setting<ushort>(moduleDefaults.FlyTextParameters.ColorKey.Value);
            configModule.TextGlowColor =
                new Setting<ushort>(moduleDefaults.FlyTextParameters.GlowColorKey.Value);
            configModule.TextItalics = new Setting<bool>(moduleDefaults.FlyTextParameters.Italics);
            return configModule;
        }

        public string GetId()
        {
            return $"{ClassJobId}{ModuleType}";
        }

        public ModuleDefaults GetModuleDefaults()
        {
            return ModuleDefaults.GetDefaultsFromType(ModuleType.Value);
        }

        public Setting<uint> ClassJobId { get; init; } = new(255);
        public Setting<ModuleType> ModuleType { get; init; } = new(Configuration.ModuleType.DirectCriticalDamage);
        public Setting<bool> PlaySound { get; set; } = new(true);
        public Setting<bool> SoundForActionsOnly { get; set; } = new(false);
        public Setting<string> FilePath { get; set; } = new(string.Empty);
        public Setting<int> Volume { get; set; } = new(12);
        public Setting<bool> ShowText { get; set; } = new(true);
        public Setting<string> Text { get; set; } = new(string.Empty);
        public Setting<ushort> TextColor { get; set; } = new(0);
        public Setting<ushort> TextGlowColor { get; set; } = new(0);
        public Setting<bool> TextItalics { get; set; } = new(false);

        public void CopySettingsFrom(ConfigModule other)
        {
            PlaySound = other.PlaySound;
            SoundForActionsOnly = other.SoundForActionsOnly;
            FilePath = other.FilePath;
            Volume = other.Volume;
            ShowText = other.ShowText;
            Text = other.Text;
            TextColor = other.TextColor;
            TextGlowColor = other.TextGlowColor;
            TextItalics = other.TextItalics;
        }
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
