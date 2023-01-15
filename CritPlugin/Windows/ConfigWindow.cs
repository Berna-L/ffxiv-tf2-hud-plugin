using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.DrunkenToad;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using ImGuiNET;

namespace CritPlugin.Windows;

public class ConfigWindow : Window, IDisposable
{
    public static String Title = "Crit Plugin Configuration";
    
    private readonly Configuration configuration;

    private readonly FileDialogManager _dialogManager;

    public ConfigWindow(CritPlugin critPlugin) : base(
        Title,
        ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.Size = new Vector2(500, 300);
        this.SizeCondition = ImGuiCond.Appearing;

        this.configuration = critPlugin.Configuration;
        _dialogManager = new FileDialogManager { AddedWindowFlags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking};
    }
    
    public override void Draw()
    {
        DrawSection(configuration.Critical);
        DrawSection(configuration.Direct);
        _dialogManager.Draw();

        if (ImGui.Button("Save"))
        {
            configuration.Save();
        }
    }

    private void DrawSection(Configuration.SubConfiguration config)
    {
        if (!ImGui.TreeNode(config.Title)) return;
        var path = config.FilePath;
        ImGui.InputText("", ref path, 512, ImGuiInputTextFlags.ReadOnly);
        ImGui.SameLine();


        void UpdatePath(bool success, List<string> paths)
        {
            if (success && paths.Count > 0)
            {
                PluginLog.Debug(config.Title);
                config.FilePath = paths[0];
            }
        }

        if (ImGuiComponents.IconButton(FontAwesomeIcon.Folder))
        {
            PluginLog.Debug(config.Title);
            _dialogManager.OpenFileDialog("Select the file", "Audio files{.wav,.mp3}", UpdatePath, 1, config.FilePath);
        }

        var volume = config.Volume;
        if (ImGui.SliderInt("Volume", ref volume, 0, 100))
        {
            config.Volume = volume;
        }

        var initialShowTextValue = config.ShowText;
        if (ImGui.Checkbox("Show text with floating damage", ref initialShowTextValue))
        {
            config.ShowText = initialShowTextValue;
        }
        ImGui.TreePop();
    }

    public override void OnClose()
    {
        _dialogManager.Reset();
    }
    
    

    public void Dispose()
    {
        _dialogManager.Reset();
    }
}
