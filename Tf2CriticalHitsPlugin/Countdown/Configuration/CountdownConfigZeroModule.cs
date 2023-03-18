using System;
using System.Collections.Generic;
using KamiLib.Configuration;
using KamiLib.ZoneFilterList;

namespace Tf2CriticalHitsPlugin.Countdown.Configuration;

public class CountdownConfigZeroModule
{
    public Setting<string> Id { get; set; } = new(Guid.NewGuid().ToString()); 
    public Setting<string> Label { get; set; } = new (string.Empty);
    public Setting<bool> Enabled { get; set; } = new(true);
    public Setting<string> FilePath { get; set; } = new(string.Empty);
    public Setting<int> Volume { get; set; } = new(100);
    public Setting<bool> ApplySfxVolume { get; set; } = new(true);
    public Setting<string> InterruptedFilePath { get; set; } = new(string.Empty);
    public Setting<int> InterruptedVolume { get; set; } = new(100);
    public Setting<bool> InterruptedApplySfxVolume { get; set; } = new(true);
    public Setting<int> MinimumCountdownTimer { get; set; } = new(5);
    public Setting<int> MaximumCountdownTimer { get; set; } = new(30);
    public Setting<bool> AllTerritories { get; set; } = new(true);
    public IList<uint> Territories { get; init; } = new List<uint>();
    public Setting<ZoneFilterTypeId> TerritoryFilterType { get; init; } = new(ZoneFilterTypeId.Whitelist);

    private CountdownConfigZeroModule()
    {
        
    }
    
    public static CountdownConfigZeroModule Create(string name)
    {
        return new CountdownConfigZeroModule
        {
            Label = new Setting<string>(name)
        };
    }
}
