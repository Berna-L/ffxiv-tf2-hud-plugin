using System.Numerics;
using ImGuiNET;

namespace Tf2CriticalHitsPlugin.Common.Windows;

public class Tf2BluScore: Tf2Window
{

    public Tf2BluScore() : base("##BluWindow", Color.Blu)
    {
        Size = new Vector2(258, 65);
    }


    public override void Draw()
    {
        ImGui.PushFont(Tf2SecondaryFont);
        ImGuiHelper.TextShadow("BLU");
        ImGui.PopFont();
        ImGuiHelper.ForegroundTextShadow(Tf2ScoreFont, "1", ImGui.GetCursorScreenPos() + new Vector2(200, -85));
        ImGui.GetWindowDrawList();
    }

}
