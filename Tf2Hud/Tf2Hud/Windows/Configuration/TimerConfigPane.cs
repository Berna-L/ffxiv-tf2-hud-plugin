using ImGuiNET;
using KamiLib.Drawing;
using Tf2Hud.Common.Configuration;
using Tf2Hud.Common.Windows;

namespace Tf2Hud.Tf2Hud.Windows.Configuration;

public class TimerConfigPane : ConfigPane
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
            .AddConfigCheckbox("Repositioning mode", configZero.Timer.RepositionMode,
                               "Enables you to move this component. Disable to use it.")
            .AddIndent(2)
            .AddString("Position:")
            .SameLine()
            .BeginDisabled(!configZero.Timer.RepositionMode)
            .AddDragFloat("##TimerXPosition", configZero.Timer.PositionX, 0, ImGui.GetMainViewport().Size.X, 100.0f)
            .SameLine()
            .AddString("x")
            .SameLine()
            .AddDragFloat("##TimerYPosition", configZero.Timer.PositionY, 0, ImGui.GetMainViewport().Size.Y, 100.0f)
            .SameLine()
            .AddButton("Default", () => configZero.Timer.RestoreDefaultPosition())
            .EndDisabled()
            .AddIndent(-2)
            .Draw();
    }
}
