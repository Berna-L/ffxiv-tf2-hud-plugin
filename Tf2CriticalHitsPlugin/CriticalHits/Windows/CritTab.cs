using System.Collections.Generic;
using System.Linq;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Utility;
using ImGuiNET;
using KamiLib;
using KamiLib.Interfaces;
using Lumina.Excel.GeneratedSheets;
using Tf2CriticalHitsPlugin.Common.Window;
using Tf2CriticalHitsPlugin.CriticalHits.Configuration;
using Tf2CriticalHitsPlugin.Windows;

namespace Tf2CriticalHitsPlugin.CriticalHits.Windows;

public class CritTab: SoundConfigurationTab<ConfigOne>
{

    public static readonly SortedDictionary<ushort, ColorInfo> ForegroundColors = new();
    public static readonly SortedDictionary<ushort, ColorInfo> GlowColors = new();

    static CritTab()
    {
        InitColors();
    }
    
    public override IEnumerable<ISelectable> GetTabSelectables()
    {
        return Configuration.JobConfigurations
                            .Values
                            .ToList()
                            .ConvertAll(jobConfig => new CritOption(jobConfig, DialogManager))
                            .OrderBy(o => o.JobConfig.GetClassJob().Role)
                            .ThenBy(o => o.JobConfig.GetClassJob().NameEnglish.ToString());
    }

    public override void DrawTabExtras()
    {
        DrawCopyButton();
    }

    private static void InitColors()
    {
        ForegroundColors.Clear();
        GlowColors.Clear();

        if (Service.DataManager != null)
        {
            var colorSheet = Service.DataManager.GetExcelSheet<UIColor>();
            if (colorSheet != null)
            {
                for (var i = 0u; i < colorSheet.RowCount; i++)
                {
                    var row = colorSheet.GetRow(i);
                    if (row != null)
                    {
                        ForegroundColors.Add((ushort)i, ColorInfo.FromUiColor((ushort)i, row.UIForeground));
                        GlowColors.Add((ushort)i, ColorInfo.FromUiColor((ushort)i, row.UIGlow));
                    }
                }
            }
        }
    }

    
    private void DrawCopyButton()
    {
        ImGui.SetCursorPosX((ImGui.GetContentRegionAvail().X / 2.0f) - (Constants.IconButtonSize * 3 / 2.0f));
        ImGui.GetContentRegionAvail();
        if (ImGuiComponents.IconButton(FontAwesomeIcon.Copy))
        {
            if (KamiCommon.WindowManager.GetWindowOfType<CritSettingsCopyWindow>() is { } window)
            {
                window.Open();
            }

        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Copy settings between jobs");
        }
        ImGui.SameLine();
        if (ImGuiComponents.IconButton(FontAwesomeIcon.FileUpload))
        {
            DialogManager.SaveFileDialog("TF2-ish Critical Hits — Share configuration...", "ZIP file{.zip}", "critical hits.zip", "zip", (b, s) =>
            {
                if (b && !s.IsNullOrEmpty())
                {
                    CreateZip(Configuration, s);
                }
            });
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Share configuration (as a ZIP)");
        }
        ImGui.SameLine();
        if (ImGuiComponents.IconButton(FontAwesomeIcon.FileDownload))
        {
            if (KamiCommon.WindowManager.GetWindowOfType<SettingsImportWindow>() is { } window)
            {
                window.IsOpen = true;
            }
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Import configuration (as a ZIP)");
        }
    }
    
    private static void CreateZip(ConfigOne configOne, string path)
    {
        configOne.CreateZip(path);
    }



    
    public CritTab(ConfigOne configuration, FileDialogManager dialogManager) : base("crits", "Critical and Direct Hits", configuration, dialogManager)
    {
        
    }
}
