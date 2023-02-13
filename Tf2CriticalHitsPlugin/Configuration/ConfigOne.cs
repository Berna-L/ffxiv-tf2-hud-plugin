using System;
using System.Collections.Generic;
using Dalamud.Configuration;
using Dalamud.Game.Gui.FlyText;
using Dalamud.Plugin;
using static Tf2CriticalHitsPlugin.Constants;
using static Tf2CriticalHitsPlugin.Constants.FlyText;

namespace Tf2CriticalHitsPlugin.Configuration;

[Serializable]
public class ConfigOne : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public IDictionary<string, SubConfiguration> SubConfigurations { get; set; } = new SortedDictionary<string, SubConfiguration>();

    public ConfigOne()
    {
        SubConfigurations.Add("DirectCriticalDamage",
                              new SubConfiguration("DirectCriticalDamage", "Direct Critical Damage",
                                                   "DIRECT CRITICAL HIT!", AutoDirectCritical,
                                                   ActionDirectCritical, DamageColor,
                                                   new FlyTextParameters(60, 7), true));
        SubConfigurations.Add("CriticalDamage",
                              new SubConfiguration("CriticalDamage", "Critical Damage",
                                                   "CRITICAL HIT!", AutoCritical, ActionCritical, DamageColor,
                                                   new FlyTextParameters(60, 7), true));
        SubConfigurations.Add("CriticalHeal",
                              new SubConfiguration("CriticalHeal", "Critical Heal",
                                                   "CRITICAL HEAL!", AutoCritical, ActionCritical, HealColor,
                                                   new FlyTextParameters(60, 7), true));
        SubConfigurations.Add("DirectDamage",
                              new SubConfiguration("DirectDamage", "Direct Damage",
                                                   "Mini crit!", AutoDirect, ActionDirect, DamageColor,
                                                   new FlyTextParameters(0, 0), false));
    }

    public class FlyTextParameters
    {
        public ushort ColorKey { get; set; }
        public ushort GlowColorKey { get; set; }

        public FlyTextParameters(ushort colorKey, ushort glowColorKey)
        {
            ColorKey = colorKey;
            GlowColorKey = glowColorKey;
        }
    }

    public class SubConfiguration
    {
        public SubConfiguration(
            string id, string sectionLabel, string defaultText, ISet<FlyTextKind> autoFlyTextKinds,
            ISet<FlyTextKind> actionFlyTextKinds, uint flyTextColor,
            FlyTextParameters textParameters, bool italics)
        {
            this.Id = id;
            this.SectionLabel = sectionLabel;
            this.Text = defaultText;
            this.AutoFlyTextKinds = autoFlyTextKinds;
            this.ActionFlyTextKinds = actionFlyTextKinds;
            this.FlyTextColor = flyTextColor;
            this.TextParameters = textParameters;
            this.DefaultTextParameters = new FlyTextParameters(textParameters.ColorKey, textParameters.GlowColorKey);
            this.Italics = italics;
        }

        [NonSerialized]
        public readonly string Id;

        [NonSerialized]
        public readonly ISet<FlyTextKind> AutoFlyTextKinds;

        [NonSerialized]
        public readonly uint FlyTextColor;

        [NonSerialized]
        public readonly ISet<FlyTextKind> ActionFlyTextKinds;

        [NonSerialized]
        public readonly string SectionLabel;

        [NonSerialized]
        public readonly FlyTextParameters DefaultTextParameters;

        public bool PlaySound { get; set; } = true;
        public bool SoundForActionsOnly { get; set; }
        public string? FilePath { get; set; }
        public int Volume { get; set; } = 12;
        
        public bool ShowText { get; set; } = true;
        public string Text { get; set; }
        public FlyTextParameters TextParameters { get; set; }
        public bool Italics { get; set; }
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
