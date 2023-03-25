using ImGuiNET;
using KamiLib.Drawing;
using Tf2Hud.Common.Configuration;
using Tf2Hud.Common.Windows;
using Tf2Hud.Tf2Hud.Configuration;

namespace Tf2Hud.Tf2Hud.Windows.Configuration;

public class WinPanelConfigPane : ConfigPane
{
    public WinPanelConfigPane(ConfigZero configZero) : base(configZero) { }

    public override void DrawLabel()
    {
        ImGui.TextColored(configZero.WinPanel.Enabled ? Colors.Green : Colors.Red, "Win Panel");
    }

    public override void Draw()
    {
        new SimpleDrawList()
            .AddConfigCheckbox("Enabled", configZero.WinPanel.Enabled)
            .AddConfigCheckbox("Repositioning mode", configZero.WinPanel.RepositionMode,
                               "Shows the component and enables the controls below. Disable to use it.")
            .AddIndent(2)
            .AddString("Position:")
            .SameLine()
            .BeginDisabled(!configZero.WinPanel.RepositionMode)
            .AddDragFloat("##WinPanelXPosition", configZero.WinPanel.PositionX, 0, ImGui.GetMainViewport().Size.X,
                          100.0f)
            .SameLine()
            .AddString("x")
            .SameLine()
            .AddDragFloat("##WinPanelYPosition", configZero.WinPanel.PositionY, 0, ImGui.GetMainViewport().Size.Y,
                          100.0f)
            .SameLine()
            .AddButton("Default", () => configZero.WinPanel.RestoreDefaultPosition())
            .EndDisabled()
            .AddIndent(-2)
            .Draw();

        InfoBox.Instance
               .AddTitle("Naming Format")
               .AddConfigRadio("Full name", configZero.WinPanel.NameDisplay, NameDisplayKind.FullName)
               .AddConfigRadio("Surname Abbreviated", configZero.WinPanel.NameDisplay,
                               NameDisplayKind.SurnameAbbreviated)
               .AddConfigRadio("Forename Abbreviated", configZero.WinPanel.NameDisplay,
                               NameDisplayKind.ForenameAbbreviated)
               .AddConfigRadio("Initials", configZero.WinPanel.NameDisplay, NameDisplayKind.Initials)
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
