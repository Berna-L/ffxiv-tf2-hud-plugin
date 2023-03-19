using System.Numerics;
using ImGuiNET;

namespace Tf2CriticalHitsPlugin.Common.Windows;

public class DetailWindow: Tf2Window
{
    public DetailWindow() : base("##Tf2DetailWindow", Color.Red)
    {
        Size = new Vector2(258 * 2, 300);
        BgAlpha = 0.8f;
    }

    public override void PreDraw()
    {
        base.PreDraw();
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 0f);
    }

    public override void Draw()
    {
        ImGui.PushFont(Tf2SecondaryFont);
        ImGui.SetCursorPosX((ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize("RED TEAM WINS!").X) / 2);
        ImGuiHelper.TextShadow("RED TEAM WINS!");
        ImGui.PopFont();
        ImGui.SetCursorPosX((ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize("RED team defeated Hesperos before the time ran out.").X) / 2);
        ImGui.Text("RED team defeated Hesperos before the time ran out.");
        ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.5f);
        ImGui.BeginChildFrame(12313, new Vector2(490, 200));
        ImGui.EndChildFrame();
        ImGui.PopStyleVar();
    }

    
    public override void PostDraw()
    {
        ImGui.PopStyleVar();
        base.PostDraw();
    }
}
