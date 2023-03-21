using System;
using System.Collections.Generic;
using System.IO;
using Dalamud.Interface;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Utility;
using ImGuiNET;
using KamiLib;
using KamiLib.Configuration;
using KamiLib.Drawing;

namespace Tf2Hud.Common.Windows;

public static class SoundDrawListExtensions
{
    public static DrawList<T> AddSoundFileConfiguration<T>(
        this DrawList<T> drawList, string id, Setting<string> filePath, Setting<int> volume,
        Setting<bool> applySfxVolume, FileDialogManager dialogManager, bool showPlayButton = false)
        where T : DrawList<T>
    {
        return drawList.AddInputString($"##{id}FilePath", filePath, 512, ImGuiInputTextFlags.ReadOnly)
                       .SameLine()
                       .AddIconButton($"{id}FileBrowse", FontAwesomeIcon.Folder,
                                      () => openFileDialog(dialogManager, filePath),
                                      "Open file browser...")
                       .StartConditional(showPlayButton)
                       .SameLine()
                       .StartConditional(!SoundEngine.IsPlaying($"{id}Test"))
                       .AddIconButton($"{id}PlayButton", FontAwesomeIcon.Play,
                                      () => SoundEngine.PlaySound(filePath.Value, applySfxVolume, volume.Value,
                                                                  $"{id}Test"))
                       .EndConditional()
                       .StartConditional(SoundEngine.IsPlaying($"{id}Test"))
                       .AddIconButton($"{id}StopButton", FontAwesomeIcon.Stop, () => SoundEngine.StopSound($"{id}Test"))
                       .EndConditional()
                       .EndConditional()
                       .AddSliderInt("Volume", volume, 0, 100)
                       .SameLine()
                       .AddConfigCheckbox("Affected by the game's sound effects volume", applySfxVolume,
                                          "If enabled, consider the volume set here to be in relation to the game's other SFX," +
                                          "\nsince the effective volume will also vary with your Master and Sound Effects volume." +
                                          "\nIf disabled, It'll always play at the set volume, even if the game is muted internally.");
    }

    private static void openFileDialog(FileDialogManager dialogManager, Setting<string> filePath)
    {
        dialogManager.OpenFileDialog(
            "Select the file", "Audio files{.wav,.mp3}", (s, p) => UpdatePath(s, p, filePath), 1,
            filePath.Value.IsNullOrEmpty()
                ? Environment.ExpandEnvironmentVariables("%USERPROFILE%")
                : Path.GetDirectoryName(filePath.Value));
    }

    private static void UpdatePath(bool success, List<string> paths, Setting<string> filePath)
    {
        if (success && paths.Count > 0)
        {
            filePath.Value = paths[0];
            KamiCommon.SaveConfiguration();
        }
    }
}
