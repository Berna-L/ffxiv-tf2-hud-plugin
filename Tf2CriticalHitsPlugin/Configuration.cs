using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace Tf2CriticalHitsPlugin
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public SubConfiguration DirectCritical = new("Direct critical hits", "DIRECT CRITICAL HIT!");
        public SubConfiguration Critical = new("Plain critical hits", "CRITICAL HIT!");
        public SubConfiguration Direct = new("Plain direct hits", "Mini crit!");
        
        public class SubConfiguration
        {
            internal SubConfiguration(String sectionTitle, String defaultText)
            {
                this.SectionTitle = sectionTitle;
                this.Text = defaultText;
            }

            [NonSerialized]
            public readonly string SectionTitle;
            public string? FilePath;
            public int Volume = 12;
            public bool PlaySound = true;
            public bool ShowText = true;
            public string Text;
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
