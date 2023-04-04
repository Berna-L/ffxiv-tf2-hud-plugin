using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Command;
using Dalamud.Interface.GameFonts;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using ImGuiNET;
using KamiLib;
using KamiLib.Configuration;
using Microsoft.Win32;
using Tf2Hud.Common;
using Tf2Hud.Common.Audio;
using Tf2Hud.Common.Configuration;
using Tf2Hud.Common.Model;
using Tf2Hud.Tf2Hud.Configuration;
using Tf2Hud.Tf2Hud.Windows;

namespace Tf2Hud.Tf2Hud;

public class Tf2HudModule : IDisposable
{
    public const string CloseWinPanel = "/tf2hudclose";
    private readonly ConfigZero configZero;
    private readonly string? tf2InstallFolder;

    private readonly Tf2WinPanel tf2WinPanel;
    private int enemyTeamScore;

    private uint lastDutyTerritory;
    private BattleNpc? lastEnemyTarget;

    private Tf2Team playerTeam = Tf2Team.Red;

    private int playerTeamScore;

    private ImFontPtr tf2Font;
    private ImFontPtr tf2ScoreFont;
    private ImFontPtr tf2SecondaryFont;


    public Tf2HudModule(ConfigZero configZero)
    {
        this.configZero = configZero;
        tf2InstallFolder = FindTf2InstallFolder();
        if (tf2InstallFolder is not null)
        {
            this.configZero.General.Tf2InstallPathAutoDetected = true;
            this.configZero.General.Tf2InstallPath = new Setting<string>(tf2InstallFolder);
        }
        else
            tf2InstallFolder = this.configZero.General.Tf2InstallPath.Value;

        CriticalCommonLib.Service.Commands.AddHandler(CloseWinPanel, new CommandInfo(OnCloseWinPanelCommand)
        {
            HelpMessage = "Closes the win panel immediately if needed."
        });

        CriticalCommonLib.Service.Interface.UiBuilder.BuildFonts += LoadTf2Fonts;

        KamiCommon.WindowManager.AddWindow(new Tf2BluScoreWindow());
        KamiCommon.WindowManager.AddWindow(new Tf2RedScoreWindow());
        KamiCommon.WindowManager.AddWindow(new Tf2MvpList());
        KamiCommon.WindowManager.AddWindow(new Tf2Timer(this.configZero));
        
        Service.DutyState.DutyStarted += OnStart;
        Service.DutyState.DutyCompleted += OnComplete;
        Service.DutyState.DutyWiped += OnWipe;
        CriticalCommonLib.Service.Framework.Update += OnUpdate;
        CriticalCommonLib.Service.ClientState.TerritoryChanged += OnTerritoryChange;

        Tf2Sound.Instance.Tf2InstallFolder = this.configZero.General.Tf2InstallPath;

        tf2WinPanel = new Tf2WinPanel(this.configZero.General, this.configZero.WinPanel, playerTeam, GetPartyList());
    }

    private static Tf2Timer? Timer => KamiCommon.WindowManager.GetWindowOfType<Tf2Timer>();

    private static Tf2MvpMember LocalTf2MvpMember
    {
        get
        {
            var localPlayer = CriticalCommonLib.Service.ClientState.LocalPlayer;
            if (localPlayer is null) return new Tf2MvpMember();
            return new Tf2MvpMember
            {
                Name = localPlayer.Name.TextValue,
                ClassJobId = localPlayer.ClassJob.Id
            };
        }
    }


    public void Dispose()
    {
        CriticalCommonLib.Service.ClientState.TerritoryChanged -= OnTerritoryChange;
        CriticalCommonLib.Service.Interface.UiBuilder.BuildFonts -= LoadTf2Fonts;
        Service.DutyState.DutyStarted -= OnStart;
        Service.DutyState.DutyCompleted -= OnComplete;
        Service.DutyState.DutyWiped -= OnWipe;
        CriticalCommonLib.Service.Framework.Update -= OnUpdate;

        CriticalCommonLib.Service.Commands.RemoveHandler(CloseWinPanel);
    }

    private void OnTerritoryChange(object? sender, ushort e)
    {
        lastEnemyTarget = null;
    }

    private void OnCloseWinPanelCommand(string command, string arguments)
    {
        tf2WinPanel.IsOpen = false;
    }

    private static string? FindTf2InstallFolder()
    {
        switch (BernaUtil.GetOperatingSystem())
        {
            case BernaUtil.OperatingSystem.Windows:
                var subkey = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
                var winSteamPath = subkey?.GetValue("SteamPath")?.ToString();
                return winSteamPath is null ? null : ReadTf2nstallPathFromLibraryVdf(winSteamPath);
            case BernaUtil.OperatingSystem.Macos:
                var macSteamPath = Directory.GetDirectories(@"Z:\Users\")
                                            .Select(p => Path.Combine(p, "Library", "Application Support", "Steam"))
                                            .FirstOrDefault(Directory.Exists);
                return macSteamPath is null ? null : ReadTf2nstallPathFromLibraryVdf(macSteamPath);
            case BernaUtil.OperatingSystem.Linux:
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
                if (installedApp.Key == "440")
                {
                    var path = libraryFolder["path"];
                    if (path is null) continue;
                    return Path.Combine(path.ToString(), "steamapps", "common", "Team Fortress 2");
                }
        }

        return null;
    }

    private void LoadTf2Fonts()
    {
        if (tf2InstallFolder is not null)
        {
            var resourceFolder = Path.Combine(tf2InstallFolder, "tf", "resource");
            tf2Font = ImGui.GetIO().Fonts
                           .AddFontFromFileTTF(Path.Combine(resourceFolder, "tf2.ttf"), 60);

            tf2ScoreFont = ImGui.GetIO().Fonts
                                .AddFontFromFileTTF(Path.Combine(resourceFolder, "tf2.ttf"), 130);
            tf2SecondaryFont = ImGui.GetIO().Fonts
                                    .AddFontFromFileTTF(Path.Combine(resourceFolder, "tf2secondary.ttf"), 40);
        }
        else
        {
            tf2Font = CriticalCommonLib.Service.Interface.UiBuilder
                                       .GetGameFontHandle(new GameFontStyle(GameFontFamily.Meidinger, 60))
                                       .ImFont;

            tf2ScoreFont = CriticalCommonLib.Service.Interface.UiBuilder
                                            .GetGameFontHandle(new GameFontStyle(GameFontFamily.Meidinger, 130))
                                            .ImFont;

            tf2SecondaryFont = CriticalCommonLib.Service.Interface.UiBuilder
                                                .GetGameFontHandle(new GameFontStyle(GameFontFamily.Axis, 40))
                                                .ImFont;
        }

        Tf2Window.UpdateFontPointers(tf2Font, tf2ScoreFont, tf2SecondaryFont);
    }

    private void OnUpdate(Framework? framework)
    {
        UpdatePointers();
        UpdateTimer();
        UpdateTarget();
        tf2WinPanel.RepositionMode = configZero.WinPanel.RepositionMode;
    }

    private void UpdatePointers()
    {
        if (tf2InstallFolder != configZero.General.Tf2InstallPath.Value) LoadTf2Fonts();
    }

    private void UpdateTimer()
    {
        if (Timer is null) return;
        var enabled = configZero.Timer.Enabled;
        var timerMoveMode = configZero.Timer.RepositionMode;
        var contentDirector = Service.ContentDirector;
        if (Service.DutyState.IsDutyStarted)
        {
            Timer.Team = playerTeam;
            Timer.IsOpen = enabled;
        }

        if (!Timer.IsOpen) Timer.IsOpen = timerMoveMode && enabled;
        if (Timer is { IsOpen: true } window)
        {
            if (contentDirector is null)
            {
                window.TimeRemaining = null;
                window.IsOpen = timerMoveMode && enabled;
            }
            else
                window.TimeRemaining = (long)Math.Floor(contentDirector.Value.ContentTimeLeft);
        }
    }

    private void OnStart(object? sender, ushort e)
    {
        playerTeam = UpdatePlayerTeam();
        if (Timer is not null)
        {
            Timer.Team = playerTeam;
            Timer.IsOpen = true;
        }

        switch (configZero.WinPanel.ScoreBehavior.Value)
        {
            case ScoreBehaviorKind.ResetEveryInstance:
                ClearScores();
                break;
            case ScoreBehaviorKind.ResetIfDutyChanged:
                if (CriticalCommonLib.Service.ClientState.TerritoryType != lastDutyTerritory) ClearScores();
                break;
            case ScoreBehaviorKind.ResetUponClosingGame:
            default:
                break;
        }

        lastDutyTerritory = CriticalCommonLib.Service.ClientState.TerritoryType;
    }

    private void ClearScores()
    {
        playerTeamScore = 0;
        enemyTeamScore = 0;
        tf2WinPanel.ClearScores();
    }

    private void OnComplete(object? sender, ushort e)
    {
        tf2WinPanel.PlayerTeam = playerTeam;
        tf2WinPanel.Show(playerTeamScore, enemyTeamScore, playerTeamScore + 1, enemyTeamScore, GetPartyList(),
                         GetEnemyName(), playerTeam);
        playerTeamScore += 1;
        SoundEngine.PlaySoundAsync(Tf2Sound.Instance.VictorySound, configZero.General.ApplySfxVolume,
                                   configZero.General.Volume.Value);
    }

    private void OnWipe(object? sender, ushort e)
    {
        tf2WinPanel.PlayerTeam = playerTeam;
        tf2WinPanel.Show(playerTeamScore, enemyTeamScore, playerTeamScore, enemyTeamScore + 1, GetPartyList(),
                         GetEnemyName(), playerTeam.Enemy);
        enemyTeamScore += 1;
        SoundEngine.PlaySoundAsync(Tf2Sound.Instance.FailSound, configZero.General.ApplySfxVolume,
                                   configZero.General.Volume.Value);
        lastEnemyTarget = null;
    }

    private Tf2Team UpdatePlayerTeam()
    {
        return configZero.General.TeamPreference.Value switch
        {
            TeamPreferenceKind.Blu => Tf2Team.Blu,
            TeamPreferenceKind.Red => Tf2Team.Red,
            TeamPreferenceKind.Random => Random.Shared.NextSingle() < 0.5 ? Tf2Team.Blu : Tf2Team.Red,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private string? GetEnemyName()
    {
        return lastEnemyTarget?.Name.TextValue;
    }

    private static List<Tf2MvpMember> GetPartyList()
    {
        if (Service.PartyList.Length == 0)
        {
            if (CriticalCommonLib.Service.ClientState.LocalPlayer is null) return new List<Tf2MvpMember>();
            return new[]
            {
                LocalTf2MvpMember
            }.ToList();
        }

        PluginLog.LogDebug($"{Service.PartyList.Length} people in the party");
        return Service.PartyList.OrderBy(pm => pm.ClassJob.GameData?.Role ?? int.MaxValue).Select(pm => new Tf2MvpMember
        {
            ClassJobId = pm.ClassJob.Id,
            Name = pm.Name.TextValue
        }).ToList();
    }

    private unsafe void UpdateTarget()
    {
        var playerId = PlayerState.Instance()->ObjectId;
        var targetObject = CriticalCommonLib.Service.Objects.FirstOrDefault(go => go.ObjectId == playerId)?.TargetObject;
        if (targetObject?.SubKind == (byte)BattleNpcSubKind.Enemy && targetObject is BattleNpc battleNpc)
        {
            if (battleNpc.Name.TextValue != lastEnemyTarget?.Name.TextValue &&
                battleNpc.MaxHp > 0.8f * (lastEnemyTarget?.MaxHp ?? 0))
            {
                lastEnemyTarget = battleNpc;
                PluginLog.Debug($"New last enemy: {lastEnemyTarget?.Name}, HP: {lastEnemyTarget?.MaxHp}");
            }
        }
    }
}
