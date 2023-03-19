using System.Numerics;
using ImGuiNET;
using Tf2CriticalHitsPlugin.Common.Windows;

namespace Tf2CriticalHitsPlugin.Tf2Hud.Windows;

public class Tf2BluScore: Tf2Window
{

    public int Score { get; set; } = 0;
    
    public Tf2BluScore() : base("##BluWindow", TeamColor.Blu)
    {
        Size = new Vector2(258, 65);
    }


    public override void Draw()
    {
        ImGui.PushFont(Tf2SecondaryFont);
        ImGuiHelper.TextShadow("BLU");
        ImGui.PopFont();
        ImGuiHelper.ForegroundTextShadow(Tf2ScoreFont, Score.ToString(), ImGui.GetCursorScreenPos() + new Vector2(180, -85));
        ImGui.GetWindowDrawList();
    }

}
