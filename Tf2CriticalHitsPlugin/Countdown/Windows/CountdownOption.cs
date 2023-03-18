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
    private readonly Setting<List<uint>> zones = new(new List<uint>());

    internal readonly CountdownConfigZeroModule Module;
    private readonly FileDialogManager dialogManager;

    public CountdownOption(CountdownConfigZeroModule module, FileDialogManager dialogManager)
    {
        this.Module = module;
        this.dialogManager = dialogManager;
        this.anywhereOrSelect = new Setting<Option>(Module.AllTerritories ? Option.Anywhere : Option.SelectTerritories);
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

    private readonly Setting<Option> anywhereOrSelect;

    public void Draw()
    {
        DrawDetailPane(dialogManager);
    }

    private void DrawDetailPane(FileDialogManager fileDialogManager)
    {

        new SimpleDrawList()
            .AddConfigCheckbox("Enabled", Module.Enabled)
            .AddInputString("Name", Module.Label, 40)
            .AddConfigRadio("Anywhere", anywhereOrSelect, Option.Anywhere)
            .AddConfigRadio("Select territories", anywhereOrSelect, Option.SelectTerritories)
            .SameLine()
            .AddHelpMarker("To know the name of a Trial/Raid arena, check its description in the Duty Finder.")
            .StartConditional(!Module.AllTerritories)
            .AddAction(() => ZoneFilterListDraw.DrawFilterTypeRadio(Module.TerritoryFilterType))
            .StartConditional(Service.ClientState.TerritoryType != 0)
            .AddAction(() => ZoneFilterListDraw.DrawAddRemoveHere(zones))
            .EndConditional()
            .AddAction(() => ZoneFilterListDraw.DrawTerritorySearch(zones, ZoneFilterType.FromId(Module.TerritoryFilterType.Value)!))
            .AddAction(() => ZoneFilterListDraw.DrawZoneList(zones, ZoneFilterType.FromId(Module.TerritoryFilterType.Value)!))
            .EndConditional()
            .Draw();

        Module.AllTerritories.Value = anywhereOrSelect.Value == Option.Anywhere;
    }

}

public enum Option
{
    Anywhere = 0,
    SelectTerritories = 1
}
