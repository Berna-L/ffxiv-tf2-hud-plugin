using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Dalamud.Configuration;
using KamiLib.ChatCommands;
using KamiLib.Configuration;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;
using Tf2CriticalHitsPlugin.Common.Configuration;
using Tf2CriticalHitsPlugin.Configuration;
using Tf2CriticalHitsPlugin.SeFunctions;
using static Tf2CriticalHitsPlugin.Constants;
using Configuration_ModuleType = Tf2CriticalHitsPlugin.Configuration.ModuleType;

namespace Tf2CriticalHitsPlugin.CriticalHits.Configuration;

[Serializable]
public class ConfigOne : BaseConfiguration
{

    public SortedDictionary<uint, JobConfig> JobConfigurations { get; set; } = new();

    public ConfigOne()
    {
        Version = 1;
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
        public ConfigModule OwnCriticalHeal { get; init; } = new();

        public IEnumerable<ConfigModule> GetModules => new[] { DirectCriticalDamage, CriticalDamage, DirectDamage, OwnCriticalHeal, OtherCriticalHeal };

        [Obsolete("Used only to import old JSONs")]
        public ConfigModule CriticalHeal
        {
            init
            {
                // Migration code from Version 2.0.0.0 configuration.
                OwnCriticalHeal = ConfigModule.Create(value.ClassJobId.Value, ModuleType.OwnCriticalHeal);
                OwnCriticalHeal.CopySettingsFrom(value);
                OtherCriticalHeal = ConfigModule.Create(value.ClassJobId.Value, ModuleType.OtherCriticalHeal);
                OtherCriticalHeal.CopySettingsFrom(value);
                if (OtherCriticalHeal.Text.Value == OwnCriticalHeal.GetModuleDefaults().DefaultText)
                {
                    OtherCriticalHeal.Text = new Setting<string>(OtherCriticalHeal.GetModuleDefaults().DefaultText);
                }

                if (OtherCriticalHeal.GameSound.Value == OwnCriticalHeal.GetModuleDefaults().GameSound)
                {
                    OtherCriticalHeal.GameSound = new Setting<Sounds>(OtherCriticalHeal.GetModuleDefaults().GameSound);
                }
            }
        }

        public ConfigModule OtherCriticalHeal { get; init; } = new();

        public ConfigModule DirectDamage { get; init; } = new();


        public static JobConfig Create(uint classJobId)
        {
            return new JobConfig
            {
                ClassJobId = new Setting<uint>(classJobId),
                DirectCriticalDamage = ConfigModule.Create(classJobId, ModuleType.DirectCriticalDamage),
                CriticalDamage = ConfigModule.Create(classJobId, ModuleType.CriticalDamage),
                OwnCriticalHeal = ConfigModule.Create(classJobId, ModuleType.OwnCriticalHeal),
                OtherCriticalHeal = ConfigModule.Create(classJobId, ModuleType.OtherCriticalHeal),
                DirectDamage = ConfigModule.Create(classJobId, ModuleType.DirectDamage)
            };
        }


        public ClassJob GetClassJob() => CombatJobs[ClassJobId.Value];

        public IEnumerator<ConfigModule> GetEnumerator()
        {
            return new[] { DirectCriticalDamage, CriticalDamage, DirectDamage, OwnCriticalHeal, OtherCriticalHeal }
                   .ToList().GetEnumerator();
        }

        public void CopySettingsFrom(JobConfig jobConfig)
        {
            DirectCriticalDamage.CopySettingsFrom(jobConfig.DirectCriticalDamage);
            CriticalDamage.CopySettingsFrom(jobConfig.CriticalDamage);
            OwnCriticalHeal.CopySettingsFrom(jobConfig.OwnCriticalHeal);
            OtherCriticalHeal.CopySettingsFrom(jobConfig.OtherCriticalHeal);
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
            configModule.GameSound = new Setting<Sounds>(moduleDefaults.GameSound);
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
        public Setting<ModuleType> ModuleType { get; init; } = new(Configuration_ModuleType.DirectCriticalDamage);
        public Setting<bool> ApplyInPvP { get; set; } = new(false);
        public Setting<bool> UseCustomFile { get; set; } = new(false);
        public Setting<bool> SoundForActionsOnly { get; set; } = new(false);
        public Setting<Sounds> GameSound { get; set; } = new(Sounds.None);
        public Setting<string> FilePath { get; set; } = new(string.Empty);
        public Setting<int> Volume { get; set; } = new(12);
        public Setting<bool> ApplySfxVolume { get; set; } = new(true);
        public Setting<bool> ShowText { get; set; } = new(true);
        public Setting<string> Text { get; set; } = new(string.Empty);
        public Setting<ushort> TextColor { get; set; } = new(0);
        public Setting<ushort> TextGlowColor { get; set; } = new(0);
        public Setting<bool> TextItalics { get; set; } = new(false);

        public void CopySettingsFrom(ConfigModule other)
        {
            ApplyInPvP = other.ApplyInPvP with { };
            UseCustomFile = other.UseCustomFile with { };
            SoundForActionsOnly = other.SoundForActionsOnly with { };
            GameSound = other.GameSound with { };
            FilePath = other.FilePath with { };
            Volume = other.Volume with { };
            ApplySfxVolume = other.ApplySfxVolume with { };
            ShowText = other.ShowText with { };
            Text = other.Text with { };
            TextColor = other.TextColor with { };
            TextGlowColor = other.TextGlowColor with { };
            TextItalics = other.TextItalics with { };
        }
    }


    public void Save()
    {
        PluginVersion = PluginVersion.Current;
        File.WriteAllText(Service.PluginInterface.ConfigFile.FullName,
                          JsonConvert.SerializeObject(this, Formatting.Indented));
    }

    public void CreateZip(string path)
    {
        var actualPath = Path.HasExtension(path) ? path : $"{path}.zip";
        var stagingPath = Path.Join(Path.GetTempPath(), "critplugin");
        foreach (var file in Directory.GetFiles(stagingPath))
        {
            File.Delete(file);
        }
        var stagingDirectory = Directory.CreateDirectory(stagingPath);
        var files = JobConfigurations.Select(c => c.Value)
                                     .SelectMany(c => c.GetModules)
                                     .Where(c => c.UseCustomFile)
                                     .Select(c => c.FilePath.ToString())
                                     .Distinct()
                                     .Where(File.Exists)
                                     .ToDictionary(s => s, s => CopyAndRenameFile(s, stagingDirectory));
        var zippedConfig = this.Clone();
        foreach (var configModule in zippedConfig.JobConfigurations.Select(c => c.Value)
                                      .SelectMany(c => c.GetModules)
                                      .Where(c => File.Exists(c.FilePath.Value)))
        {
            configModule.FilePath = new Setting<string>(files[configModule.FilePath.Value]);
        }
        File.WriteAllText(Path.Join(stagingPath, "config.json"), JsonConvert.SerializeObject(zippedConfig, Formatting.Indented));
        try
        {
            ZipFile.CreateFromDirectory(stagingPath, actualPath);
        }
        catch (IOException exception)
        {
            if (exception.Message.EndsWith("already exists."))
            {
                Chat.PrintError($"The file \"{Path.GetFileName(actualPath)}\" already exists in the chosen folder.");
            }
            throw;
        }
    }

    public static ConfigOne? GenerateFrom(string zipPath)
    {
        var zipArchive = ZipFile.OpenRead(zipPath);
        var configFile = zipArchive.GetEntry("config.json");
        if (configFile is null) return null;
        var newConfig = JsonConvert.DeserializeObject<ConfigOne>(new StreamReader(configFile.Open()).ReadToEnd());
        return newConfig;
    }

    public void ImportFrom(string zipPath, string soundsPath, ConfigOne newConfig)
    {
        var zipArchive = ZipFile.OpenRead(zipPath);
        foreach (var entry in zipArchive.Entries.Where(e => e.Name is not "config.json"))
        {
            entry.ExtractToFile(Path.Join(soundsPath, entry.Name), true);
        }
        foreach (var module in newConfig.JobConfigurations.Select(c => c.Value)
                                        .SelectMany(c => c.GetModules))
        {
            module.FilePath = new Setting<string>(Path.Join(soundsPath, module.FilePath.Value));
        }
        foreach (var jobConfig in JobConfigurations.Select(c => c.Value))
        {
            jobConfig.CopySettingsFrom(newConfig.JobConfigurations[jobConfig.ClassJobId.Value]);
        }
        Save();
    }

    private ConfigOne Clone()
    {
        var newInstance = new ConfigOne();
        newInstance.JobConfigurations.ToList()
                   .ForEach(kv => kv.Value.CopySettingsFrom(JobConfigurations[kv.Key]));
        return newInstance;
    }

    private static string CopyAndRenameFile(string originalFile, DirectoryInfo destDirectory)
    {
        var fileName = Guid.NewGuid() + Path.GetExtension(originalFile);
        File.Copy(originalFile, Path.Join(destDirectory.ToString(), fileName));
        return fileName;
    }
}
