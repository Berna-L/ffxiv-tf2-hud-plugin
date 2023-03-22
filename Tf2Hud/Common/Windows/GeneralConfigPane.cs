using System;
using System.IO;
using Dalamud.Interface;
using Dalamud.Utility;
using ImGuiNET;
using KamiLib;
using KamiLib.Configuration;
using KamiLib.Drawing;
using Tf2Hud.Common.Configuration;
using Tf2Hud.Tf2Hud;

namespace Tf2Hud.Common.Windows;

public class GeneralConfigPane: ConfigPane
{
    
    public GeneralConfigPane(ConfigZero configZero): base(configZero)
    {
    }

    
    public override void DrawLabel()
    {
        ImGui.Text("General");
    }

    public override void Draw()
    {
        InfoBox.Instance
               .AddTitle("Team")
               .AddConfigRadio(Team.Blu.Name, configZero.TeamPreference, TeamPreferenceKind.Blu)
               .SameLine()
               .AddConfigRadio(Team.Red.Name, configZero.TeamPreference, TeamPreferenceKind.Red)
               .SameLine()
               .AddConfigRadio("Randomize every instance", configZero.TeamPreference, TeamPreferenceKind.Random)
               .Draw();
        
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
