using System.Collections.Generic;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using KamiLib;
using Tf2Hud.Common.Audio;
using Tf2Hud.Common.Configuration;
using Tf2Hud.VoiceLines.Game;

namespace Tf2Hud.VoiceLines.SubModule;

public class AugmentationTokenSubModule: VoiceLinesSubModule
{
    private bool augmentationResponsePlayedHereAlready;
    private bool augmentationTokenResponsePending;
    private bool inventoryAlreadyLoaded;

    public AugmentationTokenSubModule(
        ConfigZero.GeneralConfigZero generalConfig, ConfigZero.VoiceLinesConfigZero voiceLinesConfig) : base(
        generalConfig, voiceLinesConfig, voiceLinesConfig.AugmentationToken)
    {
        Service.CriticalCommonLib.InventoryMonitor.OnInventoryChanged += OnInventoryChanged;
        CriticalCommonLib.Service.ClientState.TerritoryChanged += OnTerritoryChanged;

    }
    
    public override void Dispose()
    {
        Service.CriticalCommonLib.InventoryMonitor.OnInventoryChanged -= OnInventoryChanged;
        CriticalCommonLib.Service.ClientState.TerritoryChanged -= OnTerritoryChanged;

    }

    private void OnInventoryChanged(
        Dictionary<ulong, Dictionary<InventoryCategory, List<InventoryItem>>> inventories,
        InventoryMonitor.ItemChanges changedItems)
    {
        if (!inventoryAlreadyLoaded)
        {
            inventoryAlreadyLoaded = true;
            return;
        }

        foreach (var newItem in changedItems.NewItems)
            if (AugmentationToken.IsToken(newItem.ItemId) ||
                (AugmentationToken.IsExchangeableForToken(newItem.ItemId) &&
                 AugmentationToken.HasEnoughForToken(inventories)))
            {
                augmentationTokenResponsePending = true;
                if (Service.ContentDirector is null) // not in a duty
                    ShouldPlayAugmentationToken();
            }
    }
    
    private void OnTerritoryChanged(object? sender, ushort e)
    {
        if (augmentationTokenResponsePending)
        {
            augmentationResponsePlayedHereAlready = false;
        }
        ShouldPlayAugmentationToken();
    }

    private void ShouldPlayAugmentationToken()
    {
        if (augmentationTokenResponsePending && !augmentationResponsePlayedHereAlready)
        {
            augmentationResponsePlayedHereAlready = true;
            augmentationTokenResponsePending = false;
            if (ShouldPlay)
            {
                SoundEngine.PlaySoundAsync(Tf2Sound.Instance.RandomUpgradeStationSound, GeneralConfig.ApplySfxVolume,
                                           GeneralConfig.Volume.Value);
                VoiceLinesConfig.AugmentationToken.Heard.Value = true;
                KamiCommon.SaveConfiguration();
            }
        }
    }

}
