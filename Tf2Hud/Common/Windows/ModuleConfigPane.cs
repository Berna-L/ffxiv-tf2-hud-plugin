using ImGuiNET;
using KamiLib.Drawing;
using Tf2Hud.Common.Configuration;

namespace Tf2Hud.Common.Windows;

public abstract class ModuleConfigPane<T>: ConfigPane<T> where T: ModuleConfiguration
{
    private readonly string Name;
    
    protected ModuleConfigPane(string name, T config) : base(config)
    {
        this.Name = name;
    }

    public override void DrawLabel()
    {
        ImGui.TextColored(Config.Enabled ? Colors.Green : Colors.Red, Name);
    }
}
