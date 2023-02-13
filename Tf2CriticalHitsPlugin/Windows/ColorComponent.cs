using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;

namespace Tf2CriticalHitsPlugin.Windows;

public static class ColorComponent
{
    // Based on https://github.com/Lerofni/TooltipNotes/blob/main/TooltipNotes/Windows/ConfigWindow.cs#L116,
    // by Lerofni and mrexodia
    private static bool ColorSelector(
        string id, SortedDictionary<ushort, ColorInfo> palette, ref ushort index, ushort defaultIndex)
    {
        var paletteButtonFlags = ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.NoPicker |
                                 ImGuiColorEditFlags.NoTooltip;
        ImGui.Separator();
        ImGui.BeginGroup(); // Lock X position
        ImGui.Text("Current color:");
        ImGui.SameLine();
        ImGui.ColorButton($"{id}SelectedColor", palette[index].Vec4);
        foreach (var i in palette.Keys)
        {
            ImGui.PushID($"{id}ColorNumber{i}");
            if ((i % 15) != 0)
                ImGui.SameLine(0f, ImGui.GetStyle().ItemSpacing.Y);

            if (ImGui.ColorButton($"##{id}palette", palette[i].Vec4, paletteButtonFlags, new Vector2(20, 20)))
            {
                index = palette[i].Index;
                return true;
            }

            ImGui.PopID();
        }

        ImGui.EndGroup();


        var close = ImGui.Button("Close");
        ImGui.SameLine();
        if (ImGui.Button("Default"))
        {
            index = defaultIndex;
            return true;
        }

        return close;
    }

    public static bool SelectorButton(
        SortedDictionary<ushort, ColorInfo> palette, string id, ref ushort index, ushort defaultIndex,
        string tooltip = "")
    {
        var popupId = $"popup{id}";
        if (ImGui.ColorButton($"##pickerButton{id}", palette[index].Vec4))
        {
            ImGui.OpenPopup(popupId);
        }

        if (tooltip.Length > 0 && ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            ImGui.SetTooltip(tooltip);
        }

        if (ImGui.BeginPopup(popupId))
        {
            if (ColorSelector($"selector{id}", palette, ref index, defaultIndex))
            {
                ImGui.CloseCurrentPopup();
                return true;
            }

            ImGui.EndPopup();
        }

        return false;
    }
}
