using ImGuiNET;
using KamiLib.Drawing;
using Tf2Hud.Common.Configuration;

namespace Tf2Hud.Common.Windows;

public class TimerConfigPane: ConfigPane
{
    public TimerConfigPane(ConfigZero configZero) : base(configZero) { }
    public override void DrawLabel()
    {
        ImGui.TextColored(configZero.Timer.Enabled ? Colors.Green : Colors.Red, "Duty Timer");
    }

    public override void Draw()
    {
        new SimpleDrawList()
            .AddConfigCheckbox("Enabled", configZero.Timer.Enabled)
            .AddConfigCheckbox("Repositioning mode", configZero.Timer.MoveMode,
                               "Enables you to move this component. Disable to use it.")
            .Draw();
    }
}
