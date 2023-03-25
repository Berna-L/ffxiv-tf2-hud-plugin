using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Utility;
using ImGuiNET;
using KamiLib;
using KamiLib.Configuration;
using KamiLib.Drawing;
using Lumina.Excel.GeneratedSheets;
using Tf2Hud.Common.Audio;
using Tf2Hud.Common.Configuration;
using Tf2Hud.Common.Model;
using Tf2Hud.Common.Util;

namespace Tf2Hud.Common.Windows;

public class GeneralConfigPane : ConfigPane
{
    private const string TestSoundId = "TF2HUD+TestSound";

    private readonly Lazy<float> longestJobNameLength = new(() => Constants
                                                                  .CombatJobs.Values.Select(cj => cj.NameEnglish)
                                                                  .Select(cj => cj.ToString())
                                                                  .Select(n => ImGui.CalcTextSize(n).X)
                                                                  .OrderDescending().First());

    private readonly Audio.Audio?[] testSounds =
        { Tf2Sound.Instance.VictorySound, Tf2Sound.Instance.FailSound, Tf2Sound.Instance.RandomMannUpSound };

    public GeneralConfigPane(ConfigZero configZero) : base(configZero) { }


    public override void DrawLabel()
    {
        ImGui.Text("General");
    }

    public override void Draw()
    {
        DrawTeamSection();

        if (Service.PluginInterface.IsDev) DrawClassSection();

        DrawVolumeSection();

        DrawInstallFolder();
    }

    private void DrawTeamSection()
    {
        InfoBox.Instance
               .AddTitle("Team")
               .AddConfigRadio(Tf2Team.Blu.Name, configZero.TeamPreference, TeamPreferenceKind.Blu)
               .SameLine()
               .AddConfigRadio(Tf2Team.Red.Name, configZero.TeamPreference, TeamPreferenceKind.Red)
               .SameLine()
               .AddConfigRadio("Randomize every instance", configZero.TeamPreference, TeamPreferenceKind.Random)
               .Draw();
    }

    private void DrawClassSection()
    {
        var infoBox = InfoBox.Instance;
        infoBox
            .AddTitle("Your TF2 Class")
            .AddString("Note: This will be used in the future...", Colors.Orange)
            .AddConfigCheckbox("Set a TF2 Class per job", configZero.Class.UsePerJob,
                               "Check to define a TF2 Class per Combat Job.\nUncheck to use the same TF2 Class for all jobs.")
            .StartConditional(!configZero.Class.UsePerJob)
            .AddString("Global Class")
            .SameLine()
            .AddConfigCombo(Enum.GetValues<Tf2Class>(), configZero.Class.GlobalClass, Enum.GetName,
                            "##TF2HUD#General#GlobalClass", 100f)
            .EndConditional()
            .StartConditional(configZero.Class.UsePerJob)
            .AddAction(DrawClassComboBoxes)
            .EndConditional()
            .Draw();
    }

    private void DrawClassComboBoxes()
    {
        var simpleDrawList = new SimpleDrawList();
        foreach (var (classJob, tf2Class) in configZero.Class.ClassPerJob
                                                       .Select(kv => new KeyValuePair<ClassJob, Setting<Tf2Class>>(
                                                                   Constants.CombatJobs[kv.Key], kv.Value))
                                                       .OrderBy(kv => kv.Key.Role)
                                                       .ThenBy(kv => kv.Key.NameEnglish.ToString()))
            simpleDrawList.AddString(classJob.NameEnglish, GetJobColor(classJob))
                          .SameLine(longestJobNameLength.Value + 20)
                          .AddConfigCombo(Enum.GetValues<Tf2Class>(), tf2Class, Enum.GetName,
                                          $"##TF2HUD#General#JobClass#{classJob.Abbreviation}", 100f);
        simpleDrawList.Draw();
    }

    private static Vector4 GetJobColor(ClassJob classJob)
    {
        return classJob.Role switch
        {
            1 => Colors.Blue,
            4 => Colors.HealerGreen,
            _ => Colors.DPSRed
        };
    }


    private void DrawVolumeSection()
    {
        InfoBox.Instance
               .AddTitle("Volume")
               .AddConfigCheckbox("Affected by the game's sound effects volume", configZero.ApplySfxVolume,
                                  "If enabled, consider the volume set here to be in relation to the game's other SFX," +
                                  "\nsince the effective volume will also vary with your Master and Sound Effects volume." +
                                  "\nIf disabled, It'll always play at the set volume, even if the game is muted internally.")
               .AddSliderInt("Volume##TF2GeneralVolume", configZero.Volume, 0, 100)
               .SameLine()
               .StartConditional(IsSoundTextPlaying())
               .AddIconButton("##TF2GeneralVolumeStop", FontAwesomeIcon.Stop, StopSoundTest)
               .EndConditional()
               .StartConditional(!IsSoundTextPlaying())
               .AddIconButton("##TF2GeneralVolumePlay", FontAwesomeIcon.Play, PlaySoundTest)
               .EndConditional()
               .Draw();
    }

    private void DrawInstallFolder()
    {
        InfoBox.Instance
               .AddTitle("Team Fortress 2 install folder")
               .AddInputString("##TF2InstallFolder", configZero.Tf2InstallPath, 512, ImGuiInputTextFlags.ReadOnly)
               .StartConditional(configZero.Tf2InstallPathAutoDetected)
               .AddString("Your Team Fortress 2 install path was detected automatically!")
               .EndConditional()
               .StartConditional(!configZero.Tf2InstallPathAutoDetected)
               .SameLine()
               .AddIconButton("##TF2InstallFolderButton", FontAwesomeIcon.Folder,
                              () => openFolderDialog(configZero.Tf2InstallPath))
               .AddString("Press the folder button above to set the Team Fortress 2 install folder.\\n" +
                          "The path should end with ...steamapps\\common\\Team Fortress 2.")
               .EndConditional()
               .Draw();
    }

    private void PlaySoundTest()
    {
        var selectedSound = testSounds.Random();
        if (selectedSound is null) return;
        if (SoundEngine.IsPlaying(TestSoundId)) return;
        SoundEngine.PlaySound(selectedSound, configZero.ApplySfxVolume, configZero.Volume.Value, TestSoundId);
    }

    private static void StopSoundTest()
    {
        SoundEngine.StopSound(TestSoundId);
    }

    private bool IsSoundTextPlaying()
    {
        return SoundEngine.IsPlaying(TestSoundId);
    }

    private static void openFolderDialog(Setting<string> filePath)
    {
        CommonFileDialogManager.DialogManager.OpenFolderDialog(
            "Select the folder", (s, p) => UpdatePath(s, p, filePath),
            filePath.Value.IsNullOrEmpty()
                ? Environment.ExpandEnvironmentVariables("%USERPROFILE%")
                : Path.GetDirectoryName(filePath.Value));
    }

    private static void UpdatePath(bool success, string path, Setting<string> savedPath)
    {
        if (success && path.IsNullOrWhitespace())
        {
            savedPath.Value = path;
            KamiCommon.SaveConfiguration();
        }
    }
}
