using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Logging;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using ImGuiNET;
using KamiLib;
using Microsoft.Win32;
using Sledge.Formats.Packages;
using Tf2Hud.Common;
using Tf2Hud.Common.Windows;
using Tf2Hud.Tf2Hud.Windows;

namespace Tf2Hud.Tf2Hud;

public class Tf2HudModule : IDisposable
{
    private readonly Tf2WinPanel tf2WinPanel;
    private GameObject? lastEnemyTarget;

    private byte[]? victorySound;
    private byte[]? failSound;
    private byte[]? scoredSound;

    private ImFontPtr tf2Font;
    private ImFontPtr tf2ScoreFont;
    private ImFontPtr tf2SecondaryFont;


    private uint lastDutyTerritory;

    private int bluScore;
    private int redScore;
    private readonly string? tf2InstallFolder;


    public Tf2HudModule()
    {
        tf2InstallFolder = GetTf2InstallFolder();
        if (tf2InstallFolder == null) return;

        Service.DutyState.DutyStarted += OnStart;
        Service.DutyState.DutyCompleted += OnComplete;
        Service.DutyState.DutyWiped += OnWipe;
        Service.Framework.Update += OnUpdate;
        Service.PluginInterface.UiBuilder.BuildFonts += LoadTf2Fonts;
        LoadTf2SoundFiles();


        KamiCommon.WindowManager.AddWindow(new Tf2BluScore());
        KamiCommon.WindowManager.AddWindow(new Tf2RedScore());
        KamiCommon.WindowManager.AddWindow(new Tf2MvpList());
        KamiCommon.WindowManager.AddWindow(new Tf2Timer());

        
        tf2WinPanel = new Tf2WinPanel(scoredSound);
    }

    private static string? GetTf2InstallFolder()
    {
        switch (BernaUtil.GetOS())
        {
            case BernaUtil.OS.WINDOWS:
                var subkey = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
                var winSteamPath = subkey?.GetValue("SteamPath")?.ToString();
                return winSteamPath is null ? null : ReadTf2nstallPathFromLibraryVdf(winSteamPath);
            case BernaUtil.OS.MACOS:
                var macSteamPath = Directory.GetDirectories(@"Z:\Users\")
                                            .Select(p => Path.Combine(p, "Library", "Application Support", "Steam"))
                                            .FirstOrDefault(Directory.Exists);
                return macSteamPath is null ? null : ReadTf2nstallPathFromLibraryVdf(macSteamPath);
            case BernaUtil.OS.LINUX:
                var linuxSteamPath = Directory.GetDirectories(@"Z:\home\")
                                              .Select(p => Path.Combine(p, ".steam", "steam"))
                                              .FirstOrDefault(Directory.Exists);
                return linuxSteamPath is null ? null : ReadTf2nstallPathFromLibraryVdf(linuxSteamPath);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static string? ReadTf2nstallPathFromLibraryVdf(string steamInstallPath)
    {
        var vProperty =
            VdfConvert.Deserialize(File.ReadAllText(Path.Combine(steamInstallPath, "config", "libraryfolders.vdf")));
        foreach (var libraryFolder in vProperty.Value.Children().OfType<VProperty>().Select(v => v.Value))
        {
            var installedApps = libraryFolder["apps"];
            if (installedApps is null) continue;
            foreach (var installedApp in installedApps.Children().OfType<VProperty>())
            {
                if (installedApp.Key == "440")
                {
                    return Path.Combine(libraryFolder["path"].ToString(), "steamapps", "common", "Team Fortress 2");
                }
            }
        }

        return null;
    }

    private void LoadTf2Fonts()
    {
        var resourceFolder = Path.Combine(tf2InstallFolder, "tf", "resource");
        tf2Font = ImGui.GetIO().Fonts
                       .AddFontFromFileTTF(Path.Combine(resourceFolder, "tf2.ttf"), 60);

        tf2ScoreFont = ImGui.GetIO().Fonts
                            .AddFontFromFileTTF(Path.Combine(resourceFolder, "tf2.ttf"), 130);
        tf2SecondaryFont = ImGui.GetIO().Fonts
                                .AddFontFromFileTTF(Path.Combine(resourceFolder, "tf2secondary.ttf"), 40);
        Tf2Window.UpdateFontPointers(tf2Font, tf2ScoreFont, tf2SecondaryFont);
    }


    public void Dispose()
    {
        Service.PluginInterface.UiBuilder.BuildFonts -= LoadTf2Fonts;
        Service.DutyState.DutyStarted -= OnStart;
        Service.DutyState.DutyCompleted -= OnComplete;
        Service.DutyState.DutyWiped -= OnWipe;
        Service.Framework.Update -= OnUpdate;
    }

    private void LoadTf2SoundFiles()
    {
        var tf2VpkPath = Path.Combine(tf2InstallFolder, "tf", "tf2_sound_misc_dir.vpk");
        if (!Path.Exists(tf2VpkPath)) return;
        using var package = new VpkPackage(tf2VpkPath);

        var victory = package.Entries.First(e => e.Name == "your_team_won.wav");
        using var victoryStream = package.Open(victory);
        victorySound = new byte[victoryStream.Length];
        victoryStream.Read(victorySound, 0, victorySound.Length);

        var fail = package.Entries.First(e => e.Name == "your_team_lost.wav");
        using var failStream = package.Open(fail);
        failSound = new byte[failStream.Length];
        failStream.Read(failSound, 0, failSound.Length);

        var scored = package.Entries.First(e => e.Path == "sound/ui/scored.wav");
        using var scoredStream = package.Open(scored);
        scoredSound = new byte[scoredStream.Length];
        PluginLog.Debug(scoredSound.Length.ToString());
        scoredStream.Read(scoredSound, 0, scoredSound.Length);

        package.Dispose();
        scoredStream.Dispose();
        failStream.Dispose();
        victoryStream.Dispose();
    }

    private void OnUpdate(Framework? framework)
    {
        UpdateTimer();
        UpdateTarget();
    }

    private static unsafe void UpdateTimer()
    {
        var contentDirector = EventFramework.Instance()->GetInstanceContentDirector();
        if (KamiCommon.WindowManager.GetWindowOfType<Tf2Timer>() is { } window)
        {
            window.TimeRemaining = contentDirector is not null
                                       ? (long)Math.Round(contentDirector->ContentDirector.ContentTimeLeft)
                                       : null;
            window.IsOpen = contentDirector is not null;
        }
    }

    private void OnStart(object? sender, ushort e)
    {
        if (Service.ClientState.TerritoryType != lastDutyTerritory)
        {
            lastDutyTerritory = Service.ClientState.TerritoryType;
            bluScore = 0;
            redScore = 0;
            Tf2WinPanel.ClearScores();
        }

    }

    private void OnComplete(object? sender, ushort e)
    {
        bluScore += 1;
        OnUpdate(null);
        tf2WinPanel.Show(bluScore, redScore, GetPartyList(), GetEnemyName(), Tf2Window.TeamColor.Blu.Background);
        if (victorySound is null) return;
        SoundEngine.PlaySound(victorySound, true, 50);
    }

    private void OnWipe(object? sender, ushort e)
    {
        redScore += 1;
        tf2WinPanel.Show(bluScore, redScore, GetPartyList(), GetEnemyName(), Tf2Window.TeamColor.Red.Background);
        if (failSound is null) return;
        SoundEngine.PlaySound(failSound, true, 50);
        lastEnemyTarget = null;
    }

    private string GetEnemyName()
    {
        var enemyName = lastEnemyTarget?.Name.TextValue;
        var enemy = enemyName.IsNullOrWhitespace() ? "an anonymous enemy" : enemyName;
        return enemy;
    }

    private List<PartyMember> GetPartyList()
    {
        return Service.PartyList.OrderBy(pm => pm.ClassJob.GameData.Role).ToList();
    }

    private class Enemy
    {
        public Enemy(GameObject gameObject)
        {
            id = gameObject.ObjectId;
            name = gameObject.Name;
        }

        public uint id { get; }
        public SeString name { get; }
    }

    private unsafe void UpdateTarget()
    {
        var playerId = PlayerState.Instance()->ObjectId;
        var targetObject = Service.ObjectTable.FirstOrDefault(go => go.ObjectId == playerId)?.TargetObject;
        if (targetObject?.SubKind == (byte)BattleNpcSubKind.Enemy)
        {
            lastEnemyTarget = targetObject;
        }
    }
}
