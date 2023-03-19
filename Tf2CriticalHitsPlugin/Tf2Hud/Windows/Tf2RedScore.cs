using System.Numerics;
using ImGuiNET;

namespace Tf2CriticalHitsPlugin.Common.Windows;

public class RedWindow: Tf2Window
{
    public RedWindow() : base("##RedWindow", Color.Red)
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
        ImGuiHelper.ForegroundTextShadow(Tf2ScoreFont, "1", ImGui.GetCursorScreenPos() + new Vector2(10, -85));
        ImGui.GetWindowDrawList();
    }
}
