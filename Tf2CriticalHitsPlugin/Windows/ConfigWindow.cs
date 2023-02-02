using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using ImGuiNET;

namespace Tf2CriticalHitsPlugin.Windows;

public class ConfigWindow : Window, IDisposable
{
    public const String Title = "TF2-ish Critical Hits Configuration";

    private readonly Configuration configuration;

    private readonly FileDialogManager dialogManager;
    
    private static readonly ImGuiColorEditFlags PaletteButtonFlags = ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.NoPicker | ImGuiColorEditFlags.NoTooltip;


    public static SortedDictionary<ushort, ColorInfo> ForegroundColors = new();
    public static SortedDictionary<ushort, ColorInfo> GlowColors = new();
    
    public ConfigWindow(Tf2CriticalHitsPlugin tf2CriticalHitsPlugin) : base(Title, ImGuiWindowFlags.NoCollapse)
    {
        this.Size = new Vector2(500, 610);
        this.SizeCondition = ImGuiCond.Appearing;

        this.configuration = tf2CriticalHitsPlugin.Configuration;
        dialogManager = new FileDialogManager
            { AddedWindowFlags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking };
        dialogManager.CustomSideBarItems.Add((Environment.ExpandEnvironmentVariables("%USERNAME%"),
                                                 Environment.ExpandEnvironmentVariables("%USERPROFILE%"),
                                                 FontAwesomeIcon.User, 0));
    }

    public override void Draw()
    {
        DrawSection(configuration.DirectCritical);
        DrawSection(configuration.Critical);
        DrawSection(configuration.Direct);
        dialogManager.Draw();

        if (ImGui.Button("Save"))
        {
            configuration.Save();
        }
    }

    private void DrawSection(Configuration.SubConfiguration config)
    {
        if (!ImGui.TreeNode(config.SectionTitle)) return;
        var playSound = config.PlaySound;
        if (ImGui.Checkbox("Play sound", ref playSound))
        {
            config.PlaySound = playSound;
        }

        if (config.PlaySound)
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
                    PluginLog.Debug(config.SectionTitle);
                    config.FilePath = paths[0];
                }
            }

            if (ImGuiComponents.IconButton(FontAwesomeIcon.Folder))
            {
                PluginLog.Debug(config.SectionTitle);
                dialogManager.OpenFileDialog("Select the file", "Audio files{.wav,.mp3}", UpdatePath, 1,
                                             config.FilePath ??
                                             Environment.ExpandEnvironmentVariables("%USERPROFILE%"));
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Open file browser...");
            }
            ImGui.SameLine();

            if (ImGuiComponents.IconButton(FontAwesomeIcon.Play))
            {
                SoundEngine.PlaySound(config.FilePath, config.Volume * 0.01f);
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Preview sound");
            }

            var volume = config.Volume;
            if (ImGui.SliderInt("Volume", ref volume, 0, 100))
            {
                config.Volume = volume;
            }
            ImGui.Unindent();
        }

        var initialShowTextValue = config.ShowText;
        if (ImGui.Checkbox("Show flavor text with floating damage", ref initialShowTextValue))
        {
            config.ShowText = initialShowTextValue;
        }

        if (config.ShowText)
        {
            ImGui.Indent();
            var initialText = config.Text;
            if (ImGui.InputText("Text", ref initialText, Configuration.MaxTextLength))
            {
                config.Text = initialText;
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip($"Max. {Configuration.MaxTextLength} chars");
            }

            ImGui.Text("Color: ");
            ImGui.SameLine();
            var colorKey = config.TextColor.ColorKey;
            if (ColorComponent.SelectorButton(ForegroundColors, $"{config.Id}Foreground", ref colorKey, config.DefaultTextColor.ColorKey))
            {
                config.TextColor.ColorKey = colorKey;
            }
            
            ImGui.SameLine();
            
            ImGui.Text("Glow: ");
            ImGui.SameLine();
            var glowColorKey = config.TextColor.GlowColorKey;
            if (ColorComponent.SelectorButton(GlowColors, $"{config.Id}Glow", ref glowColorKey, config.DefaultTextColor.GlowColorKey))
            {
                config.TextColor.GlowColorKey = glowColorKey;
            }

            ImGui.SameLine();
            
            var italics = config.Italics;
            if (ImGui.Checkbox("Italics", ref italics))
            {
                config.Italics = italics;
            }

            ImGui.SameLine();
            
            if (ImGuiComponents.IconButton(FontAwesomeIcon.Eye))
            {
                Tf2CriticalHitsPlugin.GenerateTestFlyText(config);
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Test floating text");
            }
            ImGui.Unindent();
        }

        ImGui.TreePop();
    }

    public override void OnClose()
    {
        dialogManager.Reset();
    }


    public void Dispose()
    {
        dialogManager.Reset();
    }
}
