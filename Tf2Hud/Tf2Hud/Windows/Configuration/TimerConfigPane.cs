﻿using ImGuiNET;
using KamiLib.Drawing;
using Tf2Hud.Common.Configuration;
using Tf2Hud.Common.Windows;

namespace Tf2Hud.Tf2Hud.Windows.Configuration;

public class TimerConfigPane : ModuleConfigPane<ConfigZero.TimerConfigZero>
{
    public TimerConfigPane(ConfigZero.TimerConfigZero config) : base("Duty Timer", config) { }

    public override void Draw()
    {
        new SimpleDrawList()
            .AddConfigCheckbox("Enabled", Config.Enabled)
            .AddConfigCheckbox("Repositioning mode", Config.RepositionMode,
                               "Enables you to move this component. Disable to use it.")
            .AddIndent(2)
            .BeginDisabled(!Config.RepositionMode)
            .AddString("Position:")
            .SameLine()
            .AddDragFloat("##TimerXPosition", Config.PositionX, 0, ImGui.GetMainViewport().Size.X, 100.0f)
            .SameLine()
            .AddString("x")
            .SameLine()
            .AddDragFloat("##TimerYPosition", Config.PositionY, 0, ImGui.GetMainViewport().Size.Y, 100.0f)
            .SameLine()
            .AddButton("Default", () => Config.RestoreDefaultPosition())
            .AddString("Scale:")
            .SameLine()
            .AddDragFloat("##TimerScale", Config.Scale, 0, 10f, 100.0f)
            .EndDisabled()
            .AddIndent(-2)
            .Draw();
    }
}
