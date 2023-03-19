using System.Numerics;
using ImGuiNET;
using Tf2CriticalHitsPlugin.Tf2Hud.Windows;

namespace Tf2CriticalHitsPlugin.Common.Windows;

public class Tf2RedScore: Tf2Window
{
    public int Score { get; set; } = 0;

    public Tf2RedScore() : base("##RedWindow", TeamColor.Red)
    {
        Size = new Vector2(258, 65);
    }
    
    public override void Draw()
    {
        ImGui.PushFont(Tf2SecondaryFont);
        if (Size != null)
        {
            ImGui.SetCursorPosX(ImGui.GetContentRegionAvail().X + 10 - ImGui.CalcTextSize("RED").X);
        }
        ImGuiHelper.TextShadow("RED");
        ImGui.PopFont();
        ImGuiHelper.ForegroundTextShadow(Tf2ScoreFont, Score.ToString(), ImGui.GetCursorScreenPos() + new Vector2(10, -85));
        ImGui.GetWindowDrawList();
    }
}
