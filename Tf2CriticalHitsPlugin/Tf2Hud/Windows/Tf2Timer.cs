using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using ImGuiNET;
using ImPlotNET;
using KamiLib.Drawing;
using SteamDatabase.ValvePak;

namespace Tf2CriticalHitsPlugin.Common.Windows;

public class Tf2TimerWindow: Tf2Window
{
    public long? timeRemaining { private get; set; }
    public long? maxTime { private get; set; }


    public Tf2TimerWindow() : base("##Tf2TimerWindow", Color.Blu)
    {
        Size = new Vector2(220, 70);
        Position = new Vector2((ImGui.GetMainViewport().Size.X/2) - 110, 50);
    }

    public override void Draw()
    {
        if (timeRemaining is null or 0)
        {
            maxTime = null;
            return;
        };
        maxTime ??= timeRemaining;
        if (maxTime < timeRemaining)
        {
            maxTime = timeRemaining;
        }
        var text = $"{timeRemaining / 60}:{(timeRemaining % 60).ToString().PadLeft(2, '0')}";
        ImGui.PushFont(Tf2Font);
        var regionAvailable = ImGui.GetContentRegionAvail();
        var timerSize = ImGui.CalcTextSize(text);
        var circleCenter = ImGui.GetWindowPos() + new Vector2(185, 35);
        const int circleRadius = 25;
        ImGui.SetCursorPosX(((regionAvailable.X - (circleRadius*2)  - timerSize.X) / 2) + 10);
        ImGui.SetCursorPosY(((regionAvailable.Y - timerSize.Y) / 2) + 10);
        ImGui.Text(text);
        ImGui.PopFont();
        ImGui.SameLine();
        ImGui.GetForegroundDrawList().AddCircleFilled(circleCenter, circleRadius, new Vector4(49/255f, 44/255f, 41/255f, 1f).ToU32());
        var startAngle = getAngleValue(0);
        var normalizedTimeRemaining = (float)((maxTime - (maxTime - timeRemaining)) / (float) maxTime);
        var color = (normalizedTimeRemaining > 0.1f ? Colors.White : Colors.Red).ToU32();
        ImGui.GetForegroundDrawList().PathArcTo(circleCenter, circleRadius, startAngle, getAngleValue(Math.Min(0.5f, normalizedTimeRemaining)));
        ImGui.GetForegroundDrawList().PathLineTo(circleCenter);
        ImGui.GetForegroundDrawList().PathFillConvex(color);
        if (normalizedTimeRemaining >= 0.5f)
        {
            ImGui.GetForegroundDrawList().PathArcTo(circleCenter, circleRadius, getAngleValue(0.5f), getAngleValue(normalizedTimeRemaining));
            ImGui.GetForegroundDrawList().PathLineTo(circleCenter);
            ImGui.GetForegroundDrawList().PathFillConvex(color);
        }
    }


    private static float getAngleValue(float point)
    {
        return (float)(Math.PI * ((point * 360) - 90) / 180.0f);
    }
    

}
