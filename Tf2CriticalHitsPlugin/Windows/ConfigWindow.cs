using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Logging;
using ImGuiNET;
using KamiLib;
using KamiLib.Drawing;
using KamiLib.Interfaces;
using KamiLib.Windows;
using Tf2CriticalHitsPlugin.Configuration;

namespace Tf2CriticalHitsPlugin.Windows;

public class ConfigWindow : SelectionWindow, IDisposable
{
    private static readonly string PluginVersion = GetVersionText();

    private class Option : ISelectable, IDrawable
    {
        internal readonly ConfigOne.SubConfiguration subConfiguration;
        internal readonly FileDialogManager dialogManager;

        internal Option(ConfigOne.SubConfiguration subConfiguration, FileDialogManager dialogManager)
        {
            this.subConfiguration = subConfiguration;
            this.dialogManager = dialogManager;
        }


        public IDrawable Contents => this;

        public void DrawLabel()
        {
            ImGui.Text(subConfiguration.SectionLabel);

            var region = ImGui.GetContentRegionAvail();
            ImGui.SameLine(region.X - 60.0f);
            ImGuiComponents.DisabledButton(FontAwesomeIcon.Music,
                                           defaultColor: subConfiguration.PlaySound ? Colors.Green : Colors.Red);
            ImGui.SameLine();
            ImGuiComponents.DisabledButton(FontAwesomeIcon.Font,
                                           defaultColor: subConfiguration.ShowText ? Colors.Green : Colors.Red);
        }

        public string ID => subConfiguration.Id;

        public void Draw()
        {
            DrawSection(subConfiguration, dialogManager);
        }
    }

    public const String Title = "TF2-ish Critical Hits Configuration";

    private readonly ConfigOne configuration;

    internal static readonly FileDialogManager DialogManager = new()
        { AddedWindowFlags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking };

    public static readonly SortedDictionary<ushort, ColorInfo> ForegroundColors = new();
    public static readonly SortedDictionary<ushort, ColorInfo> GlowColors = new();

    public ConfigWindow(Tf2CriticalHitsPlugin tf2CriticalHitsPlugin) : base(Title, 0.2f, 55.0f)
    {
        this.configuration = tf2CriticalHitsPlugin.Configuration;
    }

    protected override IEnumerable<ISelectable> GetSelectables()
    {
        return configuration.SubConfigurations
                            .Values
                            .ToList()
                            .ConvertAll(c => new Option(c, DialogManager));
    }

    public override void Draw()
    {
        base.Draw();
        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
        DialogManager.Draw();
    }

    protected override void DrawExtras()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0.0f, 3.0f));

        // Shamelessly copied from https://github.com/MidoriKami/NoTankYou/blob/48cc7f7d750dc393c937c3df49157573198e1f48/NoTankYou/UserInterface/Windows/ConfigurationWindow.cs#L85
        if (ImGui.Button("Save", new Vector2(ImGui.GetContentRegionAvail().X, 23.0f * ImGuiHelpers.GlobalScale)))
        {
            configuration.Save();
        }

        DrawVersionText();

        ImGui.PopStyleVar();
    }

    private static void DrawSection(ConfigOne.SubConfiguration config, FileDialogManager dialogManager)
    {
        var playSound = config.PlaySound;
        if (ImGui.Checkbox("Play sound", ref playSound))
        {
            config.PlaySound = playSound;
        }

        if (config.PlaySound)
        {
            SoundSection(config, dialogManager);
        }

        var initialShowTextValue = config.ShowText;
        if (ImGui.Checkbox("Show flavor text with floating damage", ref initialShowTextValue))
        {
            config.ShowText = initialShowTextValue;
        }

        if (config.ShowText)
        {
            TextSection(config);
        }

        if (ImGui.Button("Test configuration"))
        {
            Tf2CriticalHitsPlugin.GenerateTestFlyText(config);
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Test floating text");
        }
    }

    private static void SoundSection(ConfigOne.SubConfiguration config, FileDialogManager dialogManager)
    {
        ImGui.Indent();
        var soundForActionsOnly = config.SoundForActionsOnly;
        if (ImGui.Checkbox("Play sound only for actions (ignore auto-attacks)", ref soundForActionsOnly))
        {
            config.SoundForActionsOnly = soundForActionsOnly;
        }

        var path = config.FilePath ?? "";
        ImGui.InputText("", ref path, 512, ImGuiInputTextFlags.ReadOnly);
        ImGui.SameLine();


        void UpdatePath(bool success, List<string> paths)
        {
            if (success && paths.Count > 0)
            {
                PluginLog.Debug(config.SectionLabel);
                config.FilePath = paths[0];
            }
        }

        if (ImGuiComponents.IconButton(FontAwesomeIcon.Folder))
        {
            PluginLog.Debug(config.SectionLabel);
            dialogManager.OpenFileDialog("Select the file", "Audio files{.wav,.mp3}", UpdatePath, 1,
                                         config.FilePath ??
                                         Environment.ExpandEnvironmentVariables("%USERPROFILE%"));
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Open file browser...");
        }

        var volume = config.Volume;
        if (ImGui.SliderInt("Volume", ref volume, 0, 100))
        {
            config.Volume = volume;
        }

        ImGui.Unindent();
    }

    private static void TextSection(ConfigOne.SubConfiguration config)
    {
        ImGui.Indent();
        var initialText = config.Text;
        if (ImGui.InputText("Text", ref initialText, Constants.MaxTextLength))
        {
            config.Text = initialText;
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip($"Max. {Constants.MaxTextLength} chars");
        }

        ImGui.Text("Color: ");
        ImGui.SameLine();
        var colorKey = config.TextParameters.ColorKey;
        if (ColorComponent.SelectorButton(ForegroundColors, $"{config.Id}Foreground", ref colorKey,
                                          config.DefaultTextParameters.ColorKey))
        {
            config.TextParameters.ColorKey = colorKey;
        }

        ImGui.SameLine();

        ImGui.Text("Glow: ");
        ImGui.SameLine();
        var glowColorKey = config.TextParameters.GlowColorKey;
        if (ColorComponent.SelectorButton(GlowColors, $"{config.Id}Glow", ref glowColorKey,
                                          config.DefaultTextParameters.GlowColorKey))
        {
            config.TextParameters.GlowColorKey = glowColorKey;
        }

        ImGui.SameLine();

        var italics = config.Italics;
        if (ImGui.Checkbox("Italics", ref italics))
        {
            config.Italics = italics;
        }

        ImGui.Unindent();
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
