using System.Collections.Generic;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.ImGuiFileDialog;
using ImGuiNET;
using KamiLib.Configuration;
using KamiLib.Drawing;
using KamiLib.Interfaces;
using KamiLib.ZoneFilterList;
using Tf2CriticalHitsPlugin.Countdown.Configuration;

namespace Tf2CriticalHitsPlugin.Countdown.Windows;

public class CountdownOption : ISelectable, IDrawable
{
    private readonly Setting<ZoneFilterTypeId> filterType = new(ZoneFilterType.WhiteList.Id);
    private readonly Setting<List<uint>> zones = new(new List<uint>());

    internal readonly CountdownConfigZeroModule Module;
    private readonly FileDialogManager dialogManager;

    public CountdownOption(CountdownConfigZeroModule module, FileDialogManager dialogManager)
    {
        this.Module = module;
        this.dialogManager = dialogManager;
    }

    public IDrawable Contents => this;

    public void DrawLabel()
    {
        ImGui.PushStyleColor(ImGuiCol.Text, Module.Enabled ? Colors.Green : Colors.Red);
        ImGui.Text(Module.Label.Value);
        ImGui.PopStyleColor();
        ImGui.SameLine(ImGui.GetContentRegionAvail().X - Constants.IconButtonSize);
        ImGuiComponents.IconButton(FontAwesomeIcon.Trash, defaultColor: Colors.Red);
    }

    public string ID => Module.Id.Value;

    public void Draw()
    {
        DrawDetailPane(Module, dialogManager);
    }

    private void DrawDetailPane(CountdownConfigZeroModule module, FileDialogManager fileDialogManager)
    {
        if (ImGui.Checkbox("Enabled", ref Module.Enabled.Value))
        {
            // KamiCommon.SaveConfiguration();
        }

        ImGui.InputText("Name", ref Module.Label.Value, 40);

        var anywhereOrSelect = Module.AllTerritories ? 1 : 2;
        if (ImGui.RadioButton("Anywhere", ref anywhereOrSelect, 1) ||
            ImGui.RadioButton("Select territories", ref anywhereOrSelect, 2))
        {
            Module.AllTerritories = new Setting<bool>(anywhereOrSelect == 1);
        }
        ImGui.SameLine();
        ImGuiComponents.HelpMarker("To know the name of a Trial/Raid arena, check its description in the Duty Finder.");

        if (anywhereOrSelect == 2)
        {
            ZoneFilterListDraw.DrawFilterTypeRadio(filterType);

            if (Service.ClientState.TerritoryType != 0)
            {
                ZoneFilterListDraw.DrawAddRemoveHere(zones);
            }

            ZoneFilterListDraw.DrawTerritorySearch(zones,
                                                   ZoneFilterType.FromId(filterType.Value)!);

            ZoneFilterListDraw.DrawZoneList(zones,
                                            ZoneFilterType.FromId(filterType.Value)!);
        }
    }
}
