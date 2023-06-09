using System.Linq;
using Dalamud.Interface.Raii;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using Tf2Hud.Common.Configuration;
using System;
using System.Numerics;
using System.Text;
using System.Windows.Forms;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using KamiLib.Drawing;

namespace Tf2Hud.Tf2Hud.Windows.Configuration;

public class WinPanelSavedScoresWindow: Window
{
    private readonly ConfigZero.WinPanelConfigZero winPanelConfigZero;
    public WinPanelSavedScoresWindow(ConfigZero.WinPanelConfigZero winPanelConfigZero) : base("Team Fortress Fantasy — Saved Scores")
    {
        this.winPanelConfigZero = winPanelConfigZero;
    }
    public override void Draw()
    {
        DrawScoresTable();
        DrawCsvButton();
    }

    private void DrawScoresTable()
    {
        var size = Size ?? new Vector2();
        using var child = ImRaii.Child("##ScoresTableChild", size with { Y = size.Y - ImGui.CalcTextSize("Copy CSV to Clipboard").Y - 10 });
        using var table = ImRaii.Table("##ScoresTable", 4);
        ImGui.TableNextColumn();
        ImGui.TableHeader("Duty");
        ImGui.TableNextColumn();
        ImGui.TableHeader("Your score (completions)");
        ImGui.TableNextColumn();
        ImGui.TableHeader("Enemy score (wipes)");
        ImGui.TableNextColumn();
        ImGui.TableHeader("Delete");
        foreach (var (duty, score) in winPanelConfigZero.SavedScores.OrderBy(s => s.Key))
        {
            ImGui.TableNextColumn();
            ImGui.Text(GetDuty(duty));
            ImGui.TableNextColumn();
            ImGui.Text(score.PlayerTeam.ToString());
            ImGui.TableNextColumn();
            ImGui.Text(score.EnemyTeam.ToString());
            ImGui.TableNextColumn();
            if (ImGuiComponents.IconButton(FontAwesomeIcon.Trash, Colors.SoftRed))
            {
                winPanelConfigZero.ClearScoreForDuty(duty);
            }
        }
    }

    private static string GetDuty(ushort territoryType)
    {
        return CriticalCommonLib.Service.Data.GetExcelSheet<ContentFinderCondition>()?
                   .FirstOrDefault(cfc => cfc.TerritoryType.Row == territoryType)?
                   .Name.ToString().UppercaseFirstLetter()
               ?? $"Unknown Duty of territory {territoryType}";
    }

    private void DrawCsvButton()
    {
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + ImGui.GetContentRegionAvail().Y - (ImGui.GetTextLineHeight() + 10));
        if (ImGui.Button("Copy as CSV to Clipboard"))
        {
            Clipboard.SetText(GetScoresAsCsv());
        }
    }

    private string GetScoresAsCsv()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("Territory ID,Duty Name,Player Score,Enemy Score");
        foreach (var (duty, score) in winPanelConfigZero.SavedScores.OrderBy(s => s.Key))
        {
            stringBuilder.Append(duty);
            stringBuilder.Append(',');
            stringBuilder.Append(GetDuty(duty));
            stringBuilder.Append(',');
            stringBuilder.Append(score.PlayerTeam);
            stringBuilder.Append(',');
            stringBuilder.Append(score.EnemyTeam);
            stringBuilder.AppendLine();
        }
        return stringBuilder.ToString();
    }
}

internal static class StringExtension {
    public static string UppercaseFirstLetter(this string s)
    {
        return s.Length switch
        {
            0 => s,
            1 => s[0].ToString().ToUpper(),
            _ => string.Concat(s[0].ToString().ToUpper(), s.AsSpan(1))
        };
    }
}
