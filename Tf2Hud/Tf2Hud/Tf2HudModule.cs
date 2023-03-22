using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Command;
using Dalamud.Interface.GameFonts;
using Dalamud.Logging;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using ImGuiNET;
using KamiLib;
using KamiLib.Configuration;
using Microsoft.Win32;
using Sledge.Formats.Packages;
using Tf2Hud.Common;
using Tf2Hud.Common.Configuration;
using Tf2Hud.Tf2Hud.Windows;

namespace Tf2Hud.Tf2Hud;

public class Tf2HudModule : IDisposable
{
    public const string CloseWinPanel = "/tf2hudclose";
    
    private readonly Tf2WinPanel tf2WinPanel;
    private BattleNpc? lastEnemyTarget;

    private byte[]? victorySound;
    private byte[]? failSound;
    private byte[]? scoreDingSound;

    private ImFontPtr tf2Font;
    private ImFontPtr tf2ScoreFont;
    private ImFontPtr tf2SecondaryFont;

    private Team playerTeam = Team.Red;

    private uint lastDutyTerritory;

    private int playerTeamScore;
    private int enemyTeamScore;
    private readonly string? tf2InstallFolder;
    private static readonly Tf2Timer? GetTimer = KamiCommon.WindowManager.GetWindowOfType<Tf2Timer>();
    private readonly ConfigZero configZero;


    public Tf2HudModule(ConfigZero configZero)
    {
        this.configZero = configZero;
        tf2InstallFolder = FindTf2InstallFolder();
        if (tf2InstallFolder is not null)
        {
            this.configZero.Tf2InstallPathAutoDetected = true;
            this.configZero.Tf2InstallPath = new Setting<string>(tf2InstallFolder);
        }
        else
        {
            tf2InstallFolder = this.configZero.Tf2InstallPath.Value;
        }

        Service.CommandManager.AddHandler(CloseWinPanel, new CommandInfo(OnCloseWinPanelCommand)
        {
            HelpMessage = "Closes the win panel immediately if needed."
        });
        
        Service.DutyState.DutyStarted += OnStart;
        Service.DutyState.DutyCompleted += OnComplete;
        Service.DutyState.DutyWiped += OnWipe;
        Service.Framework.Update += OnUpdate;
        Service.PluginInterface.UiBuilder.BuildFonts += LoadTf2Fonts;
        
        LoadTf2SoundFiles();

        KamiCommon.WindowManager.AddWindow(new Tf2BluScoreWindow());
        KamiCommon.WindowManager.AddWindow(new Tf2RedScoreWindow());
        KamiCommon.WindowManager.AddWindow(new Tf2MvpList());
        KamiCommon.WindowManager.AddWindow(new Tf2Timer(this.configZero));
        
        tf2WinPanel = new Tf2WinPanel(this.configZero, playerTeam, GetPartyList(), scoreDingSound);


    }

    private void OnCloseWinPanelCommand(string command, string arguments)
    {
        tf2WinPanel.IsOpen = false;
    }

    private static string? FindTf2InstallFolder()
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
            tf2Font = Service.PluginInterface.UiBuilder.GetGameFontHandle(new GameFontStyle(GameFontFamily.Meidinger, 60))
                             .ImFont;

            tf2ScoreFont = Service.PluginInterface.UiBuilder.GetGameFontHandle(new GameFontStyle(GameFontFamily.Meidinger, 130))
                                  .ImFont;

            tf2SecondaryFont = Service.PluginInterface.UiBuilder.GetGameFontHandle(new GameFontStyle(GameFontFamily.Axis, 40))
                                      .ImFont;
        }
        Tf2Window.UpdateFontPointers(tf2Font, tf2ScoreFont, tf2SecondaryFont);    

    }


    public void Dispose()
    {
        Service.PluginInterface.UiBuilder.BuildFonts -= LoadTf2Fonts;
        Service.DutyState.DutyStarted -= OnStart;
        Service.DutyState.DutyCompleted -= OnComplete;
        Service.DutyState.DutyWiped -= OnWipe;
        Service.Framework.Update -= OnUpdate;

        Service.CommandManager.RemoveHandler(CloseWinPanel);
    }

    private void LoadTf2SoundFiles()
    {
        if (tf2InstallFolder is null) return;
        var tf2VpkPath = Path.Combine(tf2InstallFolder, "tf", "tf2_sound_misc_dir.vpk");
        if (!Path.Exists(tf2VpkPath)) return;
        using var package = new VpkPackage(tf2VpkPath);

        victorySound = LoadSoundFile(package, "sound/misc/your_team_won.wav");
        failSound = LoadSoundFile(package, "sound/misc/your_team_lost.wav");
        scoreDingSound = LoadSoundFile(package, "sound/ui/scored.wav");
        
        package.Dispose();
        
    }

    private static byte[] LoadSoundFile(IPackage package, string filePath)
    {
        var file = package.Entries.First(e => e.Path == filePath);
        using var fileStream = package.Open(file);
        var result = new byte[fileStream.Length];
        fileStream.Read(result, 0, result.Length);
        fileStream.Dispose();
        return result;
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
        if (tf2InstallFolder != configZero.Tf2InstallPath.Value)
        {
            LoadTf2Fonts();
            LoadTf2SoundFiles();
        }
    }

    private unsafe void UpdateTimer()
    {
        if (GetTimer is null) return;
        var enabled = configZero.Timer.Enabled;
        var timerMoveMode = configZero.Timer.RepositionMode;
        var contentDirector = EventFramework.Instance()->GetInstanceContentDirector();
        if (Service.DutyState.IsDutyStarted)
        {
            GetTimer.Team = playerTeam;
            GetTimer.IsOpen = enabled;
        }

        if (!GetTimer.IsOpen)
        {
            GetTimer.IsOpen = timerMoveMode && enabled;
        }
        if (GetTimer is { IsOpen: true } window)
        {
            if (contentDirector is null)
            {
                window.TimeRemaining = null;
                window.IsOpen = timerMoveMode && enabled;
            }
            else
            {
                window.TimeRemaining = (long)Math.Floor(contentDirector->ContentDirector.ContentTimeLeft);
            }
        }
    }

    private void OnStart(object? sender, ushort e)
    {
        this.playerTeam = UpdatePlayerTeam();
        if (GetTimer is not null)
        {
            GetTimer.Team = playerTeam;
            GetTimer.IsOpen = true;
        }

        
        
        switch (configZero.WinPanel.ScoreBehavior.Value)
        {
            case ScoreBehaviorKind.ResetEveryInstance:
                ClearScores();
                break;
            case ScoreBehaviorKind.ResetIfDutyChanged:
                if (Service.ClientState.TerritoryType != lastDutyTerritory)
                {
                    ClearScores();
                }
                break;
            case ScoreBehaviorKind.ResetUponClosingGame:
            default:
                break;
        }
        lastDutyTerritory = Service.ClientState.TerritoryType;
    }

    private void ClearScores()
    {
        playerTeamScore = 0;
        enemyTeamScore = 0;
        tf2WinPanel.ClearScores();
    }

    private void OnComplete(object? sender, ushort e)
    {
        playerTeamScore += 1;
        tf2WinPanel.PlayerTeam = playerTeam;
        tf2WinPanel.Show(playerTeamScore, enemyTeamScore, GetPartyList(), GetEnemyName(), playerTeam);
        if (victorySound is null) return;
        SoundEngine.PlaySound(victorySound, true, 50);
    }

    private void OnWipe(object? sender, ushort e)
    {
        enemyTeamScore += 1;
        tf2WinPanel.PlayerTeam = playerTeam;
        tf2WinPanel.Show(playerTeamScore, enemyTeamScore, GetPartyList(), GetEnemyName(), playerTeam.Enemy);
        if (failSound is null) return;
        SoundEngine.PlaySound(failSound, true, 50);
        lastEnemyTarget = null;
    }

    private Team UpdatePlayerTeam()
    {
        switch (configZero.TeamPreference.Value)
        {
            case TeamPreferenceKind.Blu:
                return Team.Blu;
            case TeamPreferenceKind.Red:
                return Team.Red;
            case TeamPreferenceKind.Random:
                return Random.Shared.NextSingle() < 0.5 ? Team.Blu : Team.Red;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private string? GetEnemyName()
    {
        return lastEnemyTarget?.Name.TextValue;
    }

    private static List<Tf2MvpMember> GetPartyList()
    {
        if (Service.PartyList.Length == 0)
        {
            if (Service.ClientState.LocalPlayer is null) return new List<Tf2MvpMember>();
            return new[]
            {
                new Tf2MvpMember()
                {
                    Name = Service.ClientState.LocalPlayer.Name.TextValue,
                    ClassJobId = Service.ClientState.LocalPlayer.ClassJob.Id
                }
            }.ToList();
        }
        return Service.PartyList.OrderBy(pm => pm.ClassJob.GameData.Role).Select(pm => new Tf2MvpMember()
        {
            ClassJobId = pm.ClassJob.Id,
            Name = pm.Name.TextValue
        }).ToList();
    }

    private unsafe void UpdateTarget()
    {
        var playerId = PlayerState.Instance()->ObjectId;
        var targetObject = Service.ObjectTable.FirstOrDefault(go => go.ObjectId == playerId)?.TargetObject;
        if (targetObject?.SubKind == (byte)BattleNpcSubKind.Enemy && targetObject is BattleNpc battleNpc)
        {
            if (battleNpc.Name.TextValue != lastEnemyTarget?.Name.TextValue && battleNpc.MaxHp > 0.8f * (lastEnemyTarget?.MaxHp ?? 0))
            {
                lastEnemyTarget = battleNpc;
                PluginLog.Debug($"New last enemy: {lastEnemyTarget?.Name}, HP: {lastEnemyTarget?.MaxHp}");
            }
        }
    }
}
