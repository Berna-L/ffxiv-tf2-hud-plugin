using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Utility;
using ImGuiNET;
using KamiLib;
using KamiLib.Configuration;
using KamiLib.Drawing;
using KamiLib.Interfaces;
using Lumina.Excel.GeneratedSheets;
using Tf2CriticalHitsPlugin.Configuration;
using Tf2CriticalHitsPlugin.CriticalHits.Configuration;
using Tf2CriticalHitsPlugin.SeFunctions;
using Tf2CriticalHitsPlugin.Windows;

namespace Tf2CriticalHitsPlugin.CriticalHits.Windows;

public class CritOption : ISelectable, IDrawable
{
    internal readonly ConfigOne.JobConfig JobConfig;
    private readonly FileDialogManager dialogManager;

    internal CritOption(ConfigOne.JobConfig jobConfig, FileDialogManager dialogManager)
    {
        this.JobConfig = jobConfig;
        this.dialogManager = dialogManager;
    }


    public IDrawable Contents => this;

    public void DrawLabel()
    {
        ImGui.PushStyleColor(ImGuiCol.Text, GetJobColor(JobConfig.GetClassJob()));
        ImGui.Text(JobConfig.GetClassJob().NameEnglish);
        ImGui.PopStyleColor();
    }

    public string ID => JobConfig.GetClassJob().Abbreviation;

    public void Draw()
    {
        DrawDetailPane(JobConfig, dialogManager);
    }

    private static void DrawDetailPane(ConfigOne.JobConfig jobConfig, FileDialogManager dialogManager)
    {
        ImGui.Text($"Configuration for {jobConfig.GetClassJob().NameEnglish}");
        foreach (var module in jobConfig)
        {
            DrawConfigModule(module, dialogManager);
        }
    }

    private static void DrawConfigModule(ConfigOne.ConfigModule config, FileDialogManager dialogManager)
    {
        void UpdatePath(bool success, List<string> paths)
        {
            if (success && paths.Count > 0)
            {
                config.FilePath = new Setting<string>(paths[0]);
                KamiCommon.SaveConfiguration();
            }
        }

        InfoBox.Instance.AddTitle(config.GetModuleDefaults().SectionLabel)
               .StartConditional(config.GetModuleDefaults().SectionNote is not null)
               .AddString(config.GetModuleDefaults().SectionNote ?? "", Colors.Orange)
               .EndConditional()
               .StartConditional(config.ModuleType == ModuleType.DirectDamage)
               .AddConfigCheckbox("Apply for PvP attacks", config.ApplyInPvP,
                                  "Some Jobs show all their damage output as Direct Damage in PvP." +
                                  "\nCheck this to have the Direct Damage configuration trigger on every attack in PvP.")
               .EndConditional()
               .AddConfigCheckbox("Use custom file", config.UseCustomFile, additionalID: $"{config.GetId()}PlaySound")
               .StartConditional(!config.UseCustomFile)
               .AddIndent(2)
               .AddConfigCheckbox("Play sound only for actions (ignore auto-attacks)", config.SoundForActionsOnly)
               .AddConfigCombo(SoundsExtensions.Values(), config.GameSound, s => s.ToName(), width: 150.0f)
               .SameLine()
               .AddIconButton($"{config.GetId()}testSfx", FontAwesomeIcon.Play,
                              () => Tf2CriticalHitsPlugin.GameSoundPlayer?.Play(config.GameSound.Value))
               .SameLine()
               .AddString("(Volume is controlled by the game's settings)")
               .AddIndent(-2)
               .EndConditional()
               .StartConditional(config.UseCustomFile)
               .AddIndent(2)
               .AddConfigCheckbox("Play sound only for actions (ignore auto-attacks)", config.SoundForActionsOnly)
               .AddInputString(string.Empty, config.FilePath, 512, ImGuiInputTextFlags.ReadOnly)
               .SameLine()
               .AddIconButton($"{config.GetId()}browse", FontAwesomeIcon.Folder, () => dialogManager.OpenFileDialog(
                                  "Select the file", "Audio files{.wav,.mp3}", UpdatePath, 1,
                                  config.FilePath.Value.IsNullOrEmpty()
                                      ? Environment.ExpandEnvironmentVariables("%USERPROFILE%")
                                      : Path.GetDirectoryName(config.FilePath.Value)), "Open file browser...")
               .AddSliderInt("Volume", config.Volume, 0, 100)
               .SameLine()
               .AddConfigCheckbox("Affected by the game's sound effects volume", config.ApplySfxVolume,
                                  "If enabled, consider the volume set here to be in relation to the game's other SFX," +
                                  "\nsince the effective volume will also vary with your Master and Sound Effects volume." +
                                  "\nIf disabled, It'll always play at the set volume, even if the game is muted internally.")
               .AddIndent(-2)
               .EndConditional()
               .AddConfigCheckbox("Show flavor text with floating value", config.ShowText)
               .StartConditional(config.ShowText)
               .AddIndent(2)
               .AddInputString("Text", config.Text, Constants.MaxTextLength)
               .AddAction(() => ImGui.Text("Color: "))
               .SameLine()
               .AddAction(() =>
               {
                   if (ColorComponent.SelectorButton(CritTab.ForegroundColors, $"{config.GetId()}Foreground",
                                                     ref config.TextColor.Value,
                                                     config.GetModuleDefaults().FlyTextParameters.ColorKey.Value))
                   {
                       KamiCommon.SaveConfiguration();
                   }
               })
               .SameLine()
               .AddAction(() => ImGui.Text("Glow: "))
               .SameLine()
               .AddAction(() =>
               {
                   if (ColorComponent.SelectorButton(CritTab.GlowColors, $"{config.GetId()}Glow",
                                                     ref config.TextGlowColor.Value,
                                                     config.GetModuleDefaults().FlyTextParameters.GlowColorKey.Value))
                   {
                       KamiCommon.SaveConfiguration();
                   }
               })
               .SameLine()
               .AddConfigCheckbox("Italics", config.TextItalics)
               .AddIndent(-2)
               .EndConditional()
               .AddButton("Test configuration", () => Tf2CriticalHitsPlugin.GenerateTestFlyText(config))
               .Draw();
    }


    private static Vector4 GetJobColor(ClassJob classJob) => classJob.Role switch
    {
        1 => Colors.Blue,
        4 => Colors.HealerGreen,
        _ => Colors.DPSRed
    };
}
