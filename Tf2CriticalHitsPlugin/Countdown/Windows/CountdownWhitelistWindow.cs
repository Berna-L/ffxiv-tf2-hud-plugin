using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using KamiLib.ZoneFilterList;
using KamiLib.Configuration;
using KamiLib.Drawing;
using Microsoft.VisualBasic;

namespace Tf2CriticalHitsPlugin.Countdown.Windows;

public class CountdownWhitelistWindow : Window, IDisposable
{
    private readonly Setting<ZoneFilterTypeId> filterType = new(ZoneFilterType.WhiteList.Id);
    private readonly Setting<List<uint>> zones = new(new List<uint>());

    public CountdownWhitelistWindow() : base($"Select area and stuff")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(400, 500),
            MaximumSize = new Vector2(400, 9999),
        };

        Flags |= ImGuiWindowFlags.NoScrollbar;

        // Service.ConfigurationManager.OnCharacterDataAvailable += UpdateWindowTitle;
    }

    public void Dispose()
    {
        // Service.ConfigurationManager.OnCharacterDataAvailable -= UpdateWindowTitle;
    }

    // private void UpdateWindowTitle(object? sender, CharacterConfiguration e)
    // {
    //     WindowName = $"{Strings.Blacklist_Label} - {e.CharacterData.Name}";
    // }

    public override void Draw()
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
