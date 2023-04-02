using System;
using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using Dalamud.Game;
using Dalamud.Logging;
using KamiLib;
using Lumina.Excel.GeneratedSheets;
using Tf2Hud.Common.Audio;
using Tf2Hud.Common.Configuration;
using Tf2Hud.VoiceLines.Game;
using Tf2Hud.VoiceLines.Model;
using static Tf2Hud.Common.Configuration.ConfigZero.VoiceLinesConfigZero;

namespace Tf2Hud.VoiceLines;

public class Tf2VoiceLinesModule : IDisposable
{
    private readonly CountdownHook countdownHook;
    private readonly HashSet<int> countdownPlayedFor = new();
    private readonly CountdownState countdownState;
    private readonly ConfigZero.GeneralConfigZero generalConfig;
    private readonly ConfigZero.VoiceLinesConfigZero voiceLinesConfig;
    private bool augmentationResponsePlayedHereAlready;
    private bool augmentationTokenResponsePending;
    private bool inventoryAlreadyLoaded;

    private bool playedForFiveMinutesLeft;

    public Tf2VoiceLinesModule(
        ConfigZero.GeneralConfigZero generalConfig, ConfigZero.VoiceLinesConfigZero voiceLinesConfig)
    {
        this.generalConfig = generalConfig;
        this.voiceLinesConfig = voiceLinesConfig;
        countdownState = CountdownState.Instance();
        countdownHook = new CountdownHook(countdownState, CriticalCommonLib.Service.Condition);


        CriticalCommonLib.Service.Framework.Update += OnUpdate;

        CriticalCommonLib.Service.ClientState.TerritoryChanged += OnTerritoryChanged;

        if (Service.CriticalCommonLib.CharacterMonitor is not null)
        {
            PluginLog.Debug("opa me inscrevi aqui hein");
            Service.CriticalCommonLib.InventoryMonitor.OnInventoryChanged += OnInventoryChanged;
        }

        Service.DutyState.DutyStarted += OnStart;

        countdownState.StartCountingDown += OnStartCountingDown;
        countdownState.StopCountingDown += OnStopCountingDown;
    }


    public void Dispose()
    {
        countdownState.StopCountingDown -= OnStopCountingDown;
        countdownState.StartCountingDown -= OnStartCountingDown;

        Service.DutyState.DutyStarted -= OnStart;

        if (Service.CriticalCommonLib.InventoryMonitor is not null)
            Service.CriticalCommonLib.InventoryMonitor.OnInventoryChanged -= OnInventoryChanged;

        CriticalCommonLib.Service.ClientState.TerritoryChanged -= OnTerritoryChanged;

        CriticalCommonLib.Service.Framework.Update -= OnUpdate;

        countdownHook.Dispose();
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

    private void OnUpdate(Framework framework)
    {
        countdownHook.Update();
        UpdateCountdown();
        UpdateFiveMinutes();
    }

    private void UpdateFiveMinutes()
    {
        if (voiceLinesConfig.FiveMinutesLeft.Enabled &&
            Service.DutyState.IsDutyStarted &&
            !playedForFiveMinutesLeft &&
            (Service.ContentDirector?.ContentTimeLeft ?? int.MaxValue) < 5 * 60)
        {
            playedForFiveMinutesLeft = true;
            SoundEngine.PlaySoundAsync(Tf2Sound.Instance.FiveMinutesLeftSound, generalConfig.ApplySfxVolume,
                                       generalConfig.Volume.Value);
            voiceLinesConfig.FiveMinutesLeft.Heard.Value = true;
            KamiCommon.SaveConfiguration();
        }
    }

    private void UpdateCountdown()
    {
        if (!countdownState.CountingDown) return;
        AdministratorCountdownOngoing();
    }

    private void AdministratorCountdownOngoing()
    {
        if (!voiceLinesConfig.AdministratorCountdown.Enabled) return;
        var ceil = (int)Math.Ceiling(countdownState.CountDownValue);
        if (countdownPlayedFor.Contains(ceil))
        {
            if (countdownState.CountDownValue < 0.01f && !countdownPlayedFor.Contains(0))
            {
                countdownPlayedFor.Add(0);
                var sound = Tf2Sound.Instance.RandomGoSound;
                SoundEngine.PlaySoundAsync(sound, generalConfig.ApplySfxVolume, generalConfig.Volume.Value);
                voiceLinesConfig.AdministratorCountdown.Heard.Value = true;
                KamiCommon.SaveConfiguration();
            }
        }
        else
        {
            countdownPlayedFor.Add(ceil);
            var sound = Tf2Sound.Instance.RandomCountdownSound(ceil);
            SoundEngine.PlaySoundAsync(sound, generalConfig.ApplySfxVolume, generalConfig.Volume.Value);
        }
    }

    private void OnStartCountingDown(object? sender, EventArgs e)
    {
        countdownPlayedFor.Clear();
        UpdateCountdown();
    }

    private void OnStopCountingDown(object? sender, EventArgs e)
    {
        AdministratorCountdownZero();
    }

    private void AdministratorCountdownZero()
    {
        if (!voiceLinesConfig.MannUp.Enabled) return;
        if (!countdownState.countdownCancelled && !countdownPlayedFor.Contains(0))
        {
            countdownPlayedFor.Add(0);
            var sound = Tf2Sound.Instance.RandomGoSound;
            SoundEngine.PlaySoundAsync(sound, generalConfig.ApplySfxVolume, generalConfig.Volume.Value);
            voiceLinesConfig.AdministratorCountdown.Heard.Value = true;
            KamiCommon.SaveConfiguration();
        }
    }

    private void OnStart(object? sender, ushort e)
    {
        if (IsHighEndDuty() && ShouldPlay(voiceLinesConfig.MannUp))
        {
            SoundEngine.PlaySoundAsync(Tf2Sound.Instance.RandomMannUpSound, generalConfig.ApplySfxVolume,
                                       generalConfig.Volume.Value);
            voiceLinesConfig.MannUp.Heard.Value = true;
            KamiCommon.SaveConfiguration();
        }
    }

    private bool ShouldPlay(VoiceLineTrigger trigger)
    {
        return voiceLinesConfig.Enabled && trigger.Enabled;
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
            if (voiceLinesConfig.AugmentationToken.Enabled)
            {
                SoundEngine.PlaySoundAsync(Tf2Sound.Instance.RandomUpgradeStationSound, generalConfig.ApplySfxVolume,
                                           generalConfig.Volume.Value);
                voiceLinesConfig.AugmentationToken.Heard.Value = true;
                KamiCommon.SaveConfiguration();
            }
        }
    }


    private static bool IsHighEndDuty()
    {
        var contentDirector = Service.ContentDirector;
        if (contentDirector is null) return false;
        return CriticalCommonLib.Service.Data.GetExcelSheet<ContentFinderCondition>()?
                   .FirstOrDefault(cfc => cfc.Content == contentDirector.Value.Director.ContentId)?
                   .HighEndDuty ?? false;
    }
}
