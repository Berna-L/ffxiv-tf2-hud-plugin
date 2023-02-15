using System;
using KamiLib.ChatCommands;
using KamiLib.Configuration;
using Tf2CriticalHitsPlugin.SeFunctions;

namespace Tf2CriticalHitsPlugin.Configuration
{
    [Obsolete("Exists only for migration purposes.")]
    [Serializable]
    public class ConfigZero
    {
        public int Version { get; set; }

        public SubConfiguration DirectCritical { get; set; } = new();

        public SubConfiguration Critical { get; set; } = new();
        public SubConfiguration Direct { get; set; } = new();

        public class FlyTextColor
        {
            public ushort ColorKey { get; set; }
            public ushort GlowColorKey { get; set; }
            
        }

        public class SubConfiguration
        {
            
            public string? FilePath { get; set; }
            public int Volume { get; set; } = 12;
            public bool PlaySound { get; set; }
            public bool SoundForActionsOnly { get; set; }
            public bool ShowText { get; set; }
            public string Text { get; set; } = "";
            public FlyTextColor TextColor { get; set; } = new();
            public bool Italics { get; set; }
        }

        
        public ConfigOne MigrateToOne()
        {
            var configOne = new ConfigOne();
            var criticalHealText = Critical.Text.Equals(ModuleDefaults.GetModuleDefaultText(ModuleType.CriticalDamage))
                                       ? new Setting<string>(ModuleDefaults.GetModuleDefaultText(ModuleType.CriticalHeal))
                                       : new Setting<string>(Critical.Text);
            foreach (var jobConfig in configOne.JobConfigurations.Values)
            {
                MigrateSubConfig(DirectCritical, jobConfig.DirectCriticalDamage);
                MigrateSubConfig(Critical, jobConfig.CriticalDamage);
                MigrateSubConfig(Critical, jobConfig.CriticalHeal);
                jobConfig.CriticalHeal.Text = criticalHealText;
                MigrateSubConfig(Direct, jobConfig.DirectDamage);
            }
            Chat.Print("Update", "Your configuration has been migrated to the new version. Check the new options at /critconfig. Enjoy!");
            return configOne;
        }

        private static void MigrateSubConfig(SubConfiguration zeroSub, ConfigOne.ConfigModule oneSub)
        {
            if (zeroSub.PlaySound)
            {
                oneSub.UseCustomFile = new Setting<bool>(true);
            }
            else
            {
                oneSub.GameSound = new Setting<Sounds>(Sounds.None);
            }
            oneSub.SoundForActionsOnly = new Setting<bool>(zeroSub.SoundForActionsOnly);
            oneSub.FilePath = new Setting<string>(zeroSub.FilePath ?? string.Empty);
            oneSub.Volume = new Setting<int>(zeroSub.Volume);
            oneSub.ShowText = new Setting<bool>(zeroSub.ShowText);
            oneSub.Text = new Setting<string>(zeroSub.Text);
            oneSub.TextColor = new Setting<ushort>(zeroSub.TextColor.ColorKey);
            oneSub.TextGlowColor = new Setting<ushort>(zeroSub.TextColor.GlowColorKey);
            oneSub.TextItalics = new Setting<bool>(zeroSub.Italics);
        }
    }
}
