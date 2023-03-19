using System;
using System.Collections.Generic;
using System.Reflection;
using Dalamud.Interface;
using Dalamud.Interface.ImGuiFileDialog;
using ImGuiNET;
using KamiLib.Drawing;
using KamiLib.Interfaces;
using KamiLib.Windows;
using Tf2CriticalHitsPlugin.Common.Configuration;
using Tf2CriticalHitsPlugin.Countdown.Windows;
using Tf2CriticalHitsPlugin.CriticalHits.Windows;
using static Tf2CriticalHitsPlugin.Tf2CriticalHitsPlugin;

namespace Tf2CriticalHitsPlugin.Windows;

public class ConfigWindow : TabbedSelectionWindow, IDisposable
{

    public const String Title = $"{PluginName} — Configuration";
    private readonly IList<ISelectionWindowTab> tabs = new List<ISelectionWindowTab>();
    private static readonly string PluginVersion = GetVersionText();

    internal static readonly FileDialogManager DialogManager = new()
    {
        AddedWindowFlags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking
    };

    static ConfigWindow()
    {
        DialogManager.CustomSideBarItems.Add((Environment.ExpandEnvironmentVariables("User Folder"),
                                                 Environment.ExpandEnvironmentVariables("%USERPROFILE%"),
                                                 FontAwesomeIcon.User, 0));
    }


    public ConfigWindow(ConfigTwo config): base(Title, 55.0f)
    {
        tabs.Add(new CritTab(config.criticalHits, DialogManager));
        tabs.Add(new CountdownTab(config.countdownJams, DialogManager));
    }

    protected override IEnumerable<ISelectionWindowTab> GetTabs()
    {
        return tabs;
    }

    public override void Draw()
    {
        base.Draw();
        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
        DialogManager.Draw();
    }

    protected override void DrawWindowExtras()
    {
        DrawVersionText();
    }

    private static string GetVersionText()
    {
        var assemblyInformation = Assembly.GetExecutingAssembly().FullName!.Split(',');

        var versionString = assemblyInformation[1].Replace('=', ' ');

        var commitInfo = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                                 ?.InformationalVersion ?? "Unknown";
        return $"{versionString} - {commitInfo}";
    }

    private static void DrawVersionText()
    {
        var region = ImGui.GetContentRegionAvail();

        var versionTextSize = ImGui.CalcTextSize(PluginVersion) / 2.0f;
        var cursorStart = ImGui.GetCursorPos();
        cursorStart.X += region.X / 2.0f - versionTextSize.X;

        ImGui.SetCursorPos(cursorStart);
        ImGui.TextColored(Colors.Grey, PluginVersion);
    }


    public override void OnClose()
    {
        DialogManager.Reset();
    }


    public void Dispose()
    {
        DialogManager.Reset();
    }
}
