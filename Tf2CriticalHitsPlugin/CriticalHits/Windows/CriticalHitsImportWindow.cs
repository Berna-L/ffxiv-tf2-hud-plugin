using System.IO;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using ImGuiNET;
using KamiLib.ChatCommands;
using KamiLib.Configuration;
using Tf2CriticalHitsPlugin.CriticalHits.Configuration;
using Tf2CriticalHitsPlugin.Windows;
using static Tf2CriticalHitsPlugin.Tf2CriticalHitsPlugin;

namespace Tf2CriticalHitsPlugin.CriticalHits.Windows;

public class CriticalHitsImportWindow: Window
{
    private readonly CriticalHitsConfigOne criticalHitsConfigOne;
    private const string Title = $"{PluginName} — Critical Hits — Import settings from ZIP";
    private string zipPath = string.Empty;
    private string soundsPath = string.Empty;
    private readonly Setting<bool> makeBackup = new(false);
    private string backupPath = string.Empty;
    
    public CriticalHitsImportWindow(CriticalHitsConfigOne criticalHitsConfigOne, bool forceMainWindow = false) : base(Title, ImGuiWindowFlags.Modal | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoResize, forceMainWindow)
    {
        Size = new Vector2(700, 250);
        this.criticalHitsConfigOne = criticalHitsConfigOne;
    }
    
    
    public override void Draw()
    {
        ImGui.Text("ZIP file to import");
        ImGui.InputText(string.Empty, ref zipPath, 512, ImGuiInputTextFlags.ReadOnly);
        ImGui.SameLine();
        if (ImGuiComponents.IconButton(FontAwesomeIcon.FileArchive))
        {
            CommonFileDialogManager.DialogManager.OpenFileDialog($"{PluginName} — Critical Hits — Select ZIP file", "ZIP files{.zip}", (b, s) =>
            {
                if (b && !s.IsNullOrEmpty())
                {
                    zipPath = s;
                }
            });
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Select the source ZIP file...");
        }


        ImGui.Text("Where to save the sound files");
        ImGui.InputText(string.Empty, ref soundsPath, 512, ImGuiInputTextFlags.ReadOnly);
        ImGui.SameLine();
        if (ImGuiComponents.IconButton(FontAwesomeIcon.Folder))
        {
            CommonFileDialogManager.DialogManager.OpenFolderDialog($"{PluginName}  — Critical Hits — Select where to store the sounds", (b, s) =>
            {
                if (b && !s.IsNullOrEmpty())
                {
                    soundsPath = s;
                }
            });
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Select where to save the sound files...");
        }


        ImGui.Checkbox("Make backup", ref makeBackup.Value);
        if (makeBackup)
        {
            ImGui.Indent();
            ImGui.Text("Backup path");
            ImGui.InputText(string.Empty, ref backupPath, 512, ImGuiInputTextFlags.ReadOnly);
            ImGui.SameLine();
            if (ImGuiComponents.IconButton(FontAwesomeIcon.FileCirclePlus))
            {
                OpenBackupPathSelection();
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Select the backup file path...");
            }
            ImGui.Unindent();
        }
        if (ImGui.Button("Import"))
        {
            if (makeBackup)
            {
                if (!Path.Exists(Path.GetDirectoryName(backupPath)))
                {
                    Chat.PrintError("The defined backup path does not exist.");
                }
                criticalHitsConfigOne.CreateZip(backupPath);
            }
            CriticalHitsConfigOne.GenerateFrom(zipPath);
            IsOpen = false;
        }
    }

    private void OpenBackupPathSelection()
    {
        CommonFileDialogManager.DialogManager.SaveFileDialog($"{PluginName} — Critical Hits — Select where to save the backup",
                                                             "ZIP file{.zip}", "backup.zip", "zip",
                                                             (b, s) =>
                                                             {
                                                                 if (b && !s.IsNullOrEmpty())
                                                                 {
                                                                     backupPath = s;
                                                                 }
                                                             });
    }
}
