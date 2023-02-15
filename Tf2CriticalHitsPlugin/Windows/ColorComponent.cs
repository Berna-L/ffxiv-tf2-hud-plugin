using System.Collections.Generic;
using System.Numerics;
using Dalamud.Logging;
using ImGuiNET;

namespace Tf2CriticalHitsPlugin.Windows;

public static class ColorComponent
{
    // Based on https://github.com/Lerofni/TooltipNotes/blob/main/TooltipNotes/Windows/ConfigWindow.cs#L116,
    // by Lerofni and mrexodia
    private static bool ColorSelector(
        string id, SortedDictionary<ushort, ColorInfo> palette, ref ushort index, ushort defaultIndex)
    {
        const ImGuiColorEditFlags paletteButtonFlags = ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.NoPicker |
                                                       ImGuiColorEditFlags.NoTooltip;
        ImGui.BeginGroup(); // Lock X position
        ImGui.Text("Current color:");
        ImGui.SameLine();
        ImGui.ColorButton($"{id}SelectedColor", palette[index].Vec4);
        var shouldClose = false;
        foreach (var i in palette.Keys)
        {
            ImGui.PushID($"{id}ColorNumber{i}");
            if (i % 15 != 0)
            {
                ImGui.SameLine(0f, ImGui.GetStyle().ItemSpacing.Y);
            }

            if (ImGui.ColorButton($"##{id}palette", palette[i].Vec4, paletteButtonFlags, new Vector2(20, 20)))
            {
                index = palette[i].Index;
                shouldClose = true;
            }
            ImGui.PopID();
        }

        ImGui.EndGroup();


        shouldClose |= ImGui.Button("Close");
        ImGui.SameLine();
        if (ImGui.Button("Default"))
        {
            shouldClose = true;
            index = defaultIndex;
        }

        if (shouldClose)
        {
            ImGui.CloseCurrentPopup();
        }
        return shouldClose;
    }

    public static bool SelectorButton(
        SortedDictionary<ushort, ColorInfo> palette, string id, ref ushort index, ushort defaultIndex,
        string tooltip = "")
    {
        
        var popupId = $"popup{id}";
        var colorSelected = false;
        if (ImGui.ColorButton($"##pickerButton{id}", palette[index].Vec4))
        {
            ImGui.OpenPopup(popupId);
        }

        if (ImGui.BeginPopup(popupId))
        {
            colorSelected = ColorSelector($"##selector{id}", palette, ref index, defaultIndex);
            ImGui.EndPopup();
        }

        if (tooltip.Length > 0 && ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            ImGui.SetTooltip(tooltip);
        }
        return colorSelected;
    }
}
