using System;
using Dalamud.Plugin;
using KamiLib.ChatCommands;

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
            MigrateSubConfig(this.DirectCritical, configOne.SubConfigurations["DirectCriticalDamage"]);
            MigrateSubConfig(this.Critical, configOne.SubConfigurations["CriticalDamage"]);
            MigrateSubConfig(this.Critical, configOne.SubConfigurations["CriticalHeal"]);
            MigrateSubConfig(this.Direct, configOne.SubConfigurations["DirectDamage"]);
            Chat.Print("Update", "Your configuration has been updated. Adjustments may be needed at /critconfig. Enjoy!");
            return configOne;
        }

        private static void MigrateSubConfig(SubConfiguration zeroSub, ConfigOne.SubConfiguration oneSub)
        {
            oneSub.PlaySound = zeroSub.PlaySound;
            oneSub.SoundForActionsOnly = zeroSub.SoundForActionsOnly;
            oneSub.FilePath = zeroSub.FilePath;
            oneSub.Volume = zeroSub.Volume;

            oneSub.ShowText = zeroSub.ShowText;
            oneSub.Text = zeroSub.Text;
            oneSub.TextParameters.ColorKey = zeroSub.TextColor.ColorKey;
            oneSub.TextParameters.GlowColorKey = zeroSub.TextColor.GlowColorKey;
            oneSub.Italics = zeroSub.Italics;
        }
    }
}
