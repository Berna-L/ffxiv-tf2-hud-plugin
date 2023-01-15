using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace CritPlugin
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public SubConfiguration Critical = new("Critical and direct critical hits");
        public SubConfiguration Direct = new("Non-critical direct hits (minicrits)");
        
        public class SubConfiguration
        {
            internal SubConfiguration(String title)
            {
                this.Title = title;
            }

            [NonSerialized]
            public readonly string Title;
            public string FilePath = "";
            public int Volume = 12;
            public bool PlaySound = true;
            public bool ShowText = true;
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
