using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;
using ImGuiNET;
using KamiLib.Configuration;
using Newtonsoft.Json;
using Tf2Hud.Common.Model;
using Tf2Hud.Common.Util;
using Tf2Hud.Tf2Hud.Configuration;
using Tf2Hud.Tf2Hud.Windows;

namespace Tf2Hud.Common.Configuration;

public class ConfigZero : BaseConfiguration
{
    [NonSerialized]
    public readonly ClassConfigZero Class = new();

    public readonly GeneralConfigZero General = new();
    public readonly TimerConfigZero Timer = new();
    public readonly VoiceLinesConfigZero VoiceLines = new();
    public readonly WinPanelConfigZero WinPanel = new();

    public ConfigZero()
    {
        Version = 0;
    }

    public Setting<string> Tf2InstallPath { private get; set; } = new(string.Empty);

    public Setting<TeamPreferenceKind> TeamPreference { private get; set; } = new(TeamPreferenceKind.Random);

    public Setting<int> Volume { private get; set; } = new(50);
    public Setting<bool> ApplySfxVolume { private get; set; } = new(true);

    public void Save()
    {
        PluginVersion = PluginVersion.Current;
        File.WriteAllText(CriticalCommonLib.Service.Interface.ConfigFile.FullName,
                          JsonConvert.SerializeObject(this, Formatting.Indented));
    }

    public class GeneralConfigZero : ModuleConfiguration
    {
        public ClassConfigZero Class = new();

        [NonSerialized]
        public bool Tf2InstallPathAutoDetected;

        public Setting<string> Tf2InstallPath { get; set; } = new(string.Empty);

        public Setting<TeamPreferenceKind> TeamPreference { get; set; } = new(TeamPreferenceKind.Random);

        public Setting<int> Volume { get; set; } = new(50);

        public Setting<bool> ApplySfxVolume { get; set; } = new(true);

        public void UpdateFromOldVersion(ConfigZero configZero)
        {
            Tf2InstallPath = configZero.Tf2InstallPath;
            Class = configZero.Class;
            TeamPreference = configZero.TeamPreference;
            Volume = configZero.Volume;
            ApplySfxVolume = configZero.ApplySfxVolume;
        }
    }

    public class ClassConfigZero
    {
        public readonly IDictionary<uint, Setting<Tf2Class>> ClassPerJob = InitializeClassDictionary();
        public readonly Setting<bool> UsePerJob = new(false);
        public Setting<Tf2Class> GlobalClass { get; set; } = new(Enum.GetValues<Tf2Class>().Random());

        public int Version { get; set; } = 0;

        private static IDictionary<uint, Setting<Tf2Class>> InitializeClassDictionary()
        {
            return Constants.CombatJobs.Select(
                                c => new KeyValuePair<uint, Tf2Class>(
                                    c.Key, Tf2ClassHelper.GetTf2ClassFromXivCombatClass(c.Value)))
                            .ToDictionary(it => it.Key, it => new Setting<Tf2Class>(it.Value));
        }

        public Tf2Class? CurrentClass(PlayerCharacter localPlayer)
        {
            return UsePerJob ? ClassPerJob[localPlayer.ClassJob.Id].Value : GlobalClass.Value;
        }
    }

    public class TimerConfigZero : WindowModuleConfiguration
    {
        public override float GetPositionXDefault()
        {
            return (ImGui.GetMainViewport().Size.X / 2) - 110;
        }

        public override float GetPositionYDefault()
        {
            return 50;
        }
    }

    public class WinPanelConfigZero : WindowModuleConfiguration
    {
        public WinPanelConfigZero()
        {
            Version = 0;
        }

        public Setting<ScoreBehaviorKind> ScoreBehavior { get; set; } = new(ScoreBehaviorKind.ResetIfDutyChanged);
        public Setting<int> TimeToClose { get; set; } = new(10);

        public Setting<NameDisplayKind> NameDisplay { get; set; } = new(NameDisplayKind.FullName);

        public override float GetPositionXDefault()
        {
            return (ImGui.GetMainViewport().Size.X / 2) - Tf2Window.ScorePanelWidth;
        }

        public override float GetPositionYDefault()
        {
            return ImGui.GetMainViewport().Size.Y - 500;
        }
    }

    public class VoiceLinesConfigZero : ModuleConfiguration
    {
        public readonly VoiceLineTrigger AdministratorCountdown = new("Administrator Countdown",
                                                                      "Makes the Administrator count down with a countdown. Wow!" +
                                                                      "\nHas rare chance of being extra spicy if you have friends with you.");

        public readonly VoiceLineTrigger AugmentationToken = new("Upgrade Station with Augmentation Token",
                                                                 "Plays a random Administrator voice line from MvM after getting" +
                                                                 "\nan augmentation token for the current Savage Raid series" +
                                                                 "\nor having enough books or coins to exchange for one.");

        public readonly VoiceLineTrigger FiveMinutesLeft = new("Five minutes left",
                                                               "Reminds you when it's five minutes left in the so-called \"Mission\".");

        public readonly VoiceLineTrigger MannUp = new("Mann Up when starting a High-End Duty",
                                                      "Plays a random Administrator voice line from Mann Up mode" +
                                                      "\nwhen starting a duty classified as High-End in the Duty Finder.");

        public readonly VoiceLineTrigger Revive = new("Resurrection",
                                                      "Plays a random MvM revive line for your TF2 Class" +
                                                      "\nif you get revived by another player.");

        public readonly VoiceLineTrigger LastOneAlive = new("Last one alive",
                                                            "The Administrator will tell you if you are" +
                                                            "\nthe last one alive in your party.");


        public VoiceLinesConfigZero()
        {
            Enabled = new Setting<bool>(false);
        }

        public Setting<bool> SurpriseMe { get; set; } = new(true);

        public class VoiceLineTrigger : ModuleConfiguration
        {
            [NonSerialized]
            public readonly string Description;

            [NonSerialized]
            public readonly string Name;

            public VoiceLineTrigger(string name, string description)
            {
                Version = 0;
                Name = name;
                Description = description;
            }

            public Setting<bool> Heard { get; set; } = new(false);
            public VoiceLineTrigger[] SubTriggers { get; set; } = Array.Empty<VoiceLineTrigger>();
        }
    }
}
