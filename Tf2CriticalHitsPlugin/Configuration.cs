using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace Tf2CriticalHitsPlugin
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;
        public const int MaxTextLength = 40;

        public SubConfiguration DirectCritical =
            new("directCritical", "Direct critical hits", "DIRECT CRITICAL HIT!", new FlyTextColor(60, 7), true);

        public SubConfiguration Critical = new("critical", "Plain critical hits", "CRITICAL HIT!", new FlyTextColor(60, 7), true);
        public SubConfiguration Direct = new("direct", "Plain direct hits", "Mini crit!", new FlyTextColor(0, 0), false);

        public class FlyTextColor
        {
            public ushort ColorKey;
            public ushort GlowColorKey;

            public FlyTextColor(ushort colorKey, ushort glowColorKey)
            {
                ColorKey = colorKey;
                GlowColorKey = glowColorKey;
            }
        }

        public class SubConfiguration
        {
            internal SubConfiguration(
                string id, string sectionTitle, string defaultText, FlyTextColor textColor, bool italics)
            {
                this.SectionTitle = sectionTitle;
                this.Text = defaultText;
                this.TextColor = textColor;
                DefaultTextColor = new FlyTextColor(textColor.ColorKey, textColor.GlowColorKey);
                Italics = italics;
                this.Id = id;
            }

            [NonSerialized]
            public readonly string Id;
            [NonSerialized]
            public readonly string SectionTitle;
            public string? FilePath;
            public int Volume = 12;
            public bool PlaySound = true;
            public bool SoundForActionsOnly = false;
            public bool ShowText = true;
            public string Text;
            public readonly FlyTextColor TextColor;
            [NonSerialized]
            public readonly FlyTextColor DefaultTextColor;
            public bool Italics;
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
}
