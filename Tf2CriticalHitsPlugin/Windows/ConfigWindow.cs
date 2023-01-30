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
    public static readonly String Title = "TF2-ish Critical Hits Configuration";
    
    private readonly Configuration configuration;

    private readonly FileDialogManager dialogManager;

    public ConfigWindow(Tf2CriticalHitsPlugin tf2CriticalHitsPlugin) : base(
        Title,
        ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.Size = new Vector2(500, 550);
        this.SizeCondition = ImGuiCond.Appearing;

        this.configuration = tf2CriticalHitsPlugin.Configuration;
        dialogManager = new FileDialogManager { AddedWindowFlags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking};
        dialogManager.CustomSideBarItems.Add((Environment.ExpandEnvironmentVariables("%USERNAME%"), Environment.ExpandEnvironmentVariables("%USERPROFILE%"), FontAwesomeIcon.User, 0));
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
        }

        var initialShowTextValue = config.ShowText;
        if (ImGui.Checkbox("Show flavor text with floating damage", ref initialShowTextValue))
        {
            config.ShowText = initialShowTextValue;
        }

        if (config.ShowText)
        {
            var initialText = config.Text;
            if (ImGui.InputText("Text", ref initialText, 20))
            {
                config.Text = initialText;
            }
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
