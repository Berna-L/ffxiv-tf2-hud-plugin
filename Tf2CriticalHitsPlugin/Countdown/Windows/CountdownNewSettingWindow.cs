using Dalamud.Utility;
using ImGuiNET;
using KamiLib.Configuration;
using Tf2CriticalHitsPlugin.Countdown.Configuration;

namespace Tf2CriticalHitsPlugin.Countdown.Windows;

public class CountdownNewSettingWindow: Dalamud.Interface.Windowing.Window
{
    private readonly CountdownConfigZero configuration;
    private Setting<string> NewName { get; set; } = new Setting<string>(string.Empty);
    public const string Title = "Countdown Jams — Naming new configuration";

    public CountdownNewSettingWindow(CountdownConfigZero configuration, ImGuiWindowFlags flags = ImGuiWindowFlags.None, bool forceMainWindow = false) : base(
        Title, flags, forceMainWindow)
    {
        this.configuration = configuration;
    } 
    
    public override void Draw()
    {
        ImGui.InputText("Name", ref NewName.Value, 50);
        if (NewName.Value.Trim().IsNullOrEmpty())
        {
            ImGui.BeginDisabled();
        }
        if (ImGui.Button("Create"))
        {
            configuration.modules.Add(CountdownConfigZeroModule.Create(NewName.Value));
            IsOpen = false;
        }

        if (NewName.Value.Trim().IsNullOrEmpty())
        {
            ImGui.EndDisabled();
        }
    }

    public void Open()
    {
        NewName = new Setting<string>(string.Empty);
        IsOpen = true;
    }
}
