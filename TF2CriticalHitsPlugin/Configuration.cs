using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace Tf2CriticalHitsPlugin
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public SubConfiguration Critical = new("Critical and direct critical hits", "CRITICAL HIT!");
        public SubConfiguration Direct = new("Non-critical direct hits (minicrits)", "Mini crit!");
        
        public class SubConfiguration
        {
            internal SubConfiguration(String title, String text)
            {
                this.Title = title;
                this.Text = text;
            }

            [NonSerialized]
            public readonly string Title;
            public string FilePath = "";
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
