using System;
using KamiLib.Configuration;
using Tf2CriticalHitsPlugin.Configuration;
using Tf2CriticalHitsPlugin.SeFunctions;

namespace Tf2CriticalHitsPlugin.CriticalHits.Configuration
{
    [Obsolete("Exists only for migration purposes.")]
    [Serializable]
    public class CriticalHitsConfigZero
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

        
        public CriticalHitsConfigOne MigrateToOne()
        {
            var configOne = new CriticalHitsConfigOne();
            var ownCriticalHealText = Critical.Text.Equals(ModuleDefaults.GetModuleDefaultText(ModuleType.CriticalDamage))
                                       ? new Setting<string>(ModuleDefaults.GetModuleDefaultText(ModuleType.OwnCriticalHeal))
                                       : new Setting<string>(Critical.Text);
            var otherCriticalHealText = Critical.Text.Equals(ModuleDefaults.GetModuleDefaultText(ModuleType.CriticalDamage))
                                          ? new Setting<string>(ModuleDefaults.GetModuleDefaultText(ModuleType.OtherCriticalHeal))
                                          : new Setting<string>(Critical.Text);

            foreach (var jobConfig in configOne.JobConfigurations.Values)
            {
                MigrateSubConfig(DirectCritical, jobConfig.DirectCriticalDamage);
                MigrateSubConfig(Critical, jobConfig.CriticalDamage);
                MigrateSubConfig(Critical, jobConfig.OwnCriticalHeal);
                jobConfig.OwnCriticalHeal.Text = ownCriticalHealText;
                MigrateSubConfig(Critical, jobConfig.OtherCriticalHeal);
                jobConfig.OtherCriticalHeal.Text = otherCriticalHealText;
                MigrateSubConfig(Direct, jobConfig.DirectDamage);
            }
            return configOne;
        }

        private static void MigrateSubConfig(SubConfiguration zeroSub, CriticalHitsConfigOne.ConfigModule oneSub)
        {
            if (zeroSub.PlaySound)
            {
                oneSub.UseCustomFile = new Setting<bool>(zeroSub.FilePath != string.Empty);
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
