using ImGuiNET;
using KamiLib.Drawing;
using Tf2Hud.Common.Configuration;
using Tf2Hud.Tf2Hud;

namespace Tf2Hud.Common.Windows;

public class WinPanelConfigPane: ConfigPane
{
    public WinPanelConfigPane(ConfigZero configZero): base(configZero)
    {
    }

    public override void DrawLabel()
    {
        ImGui.TextColored(configZero.WinPanel.Enabled ? Colors.Green : Colors.Red, "Win Panel");
    }

    public override void Draw()
    {
        new SimpleDrawList()
            .AddConfigCheckbox("Enabled", configZero.WinPanel.Enabled)
            .AddConfigCheckbox("Repositioning mode", configZero.WinPanel.RepositionMode,
                               "Enables you to move this component. Disable to use it.")
            .Draw();

        InfoBox.Instance
               .AddTitle("Scoring persistence")
               .AddConfigRadio("Reset scores when I enter an instance of a different duty than the last",
                               configZero.WinPanel.ScoreBehavior, ScoreBehaviorKind.ResetIfDutyChanged,
                               "The team scores will be saved between instances of the same duty.\nIf you enter an instance of a different duty, they'll be reset.\n\nThe scores aren't saved between play sessions.")
               .AddConfigRadio("Reset scores when I enter any instance", configZero.WinPanel.ScoreBehavior,
                               ScoreBehaviorKind.ResetEveryInstance,
                               "The team scores will reset whenever you join an instance, even if it's for the same duty as the last one.")
               .AddConfigRadio("Reset scores when I close the game", configZero.WinPanel.ScoreBehavior,
                               ScoreBehaviorKind.ResetUponClosingGame,
                               "The team scores will only reset upon closing the game.")
               .Draw();

        InfoBox.Instance
               .AddTitle("Time to close")
               .AddString("Close Win Panel")
               .SameLine()
               .AddInputInt("##WinPanelTimeToClose", configZero.WinPanel.TimeToClose, 0, 60)
               .SameLine()
               .AddString("seconds after it appears.")
               .AddString($"You can also use {Tf2HudModule.CloseWinPanel} to close it manually.")
               .Draw();
        
        
    }

}
