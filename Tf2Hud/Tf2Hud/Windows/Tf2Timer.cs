using System;
using System.Numerics;
using ImGuiNET;
using KamiLib.Drawing;
using Tf2Hud.Common.Configuration;

namespace Tf2Hud.Tf2Hud.Windows;

public class Tf2Timer : Tf2Window
{
    private readonly ConfigZero configZero;
    private static readonly Vector2 DefaultPosition = new((ImGui.GetMainViewport().Size.X / 2) - 110, 50);

    public Tf2Timer(ConfigZero configZero) : base("##Tf2Timer", Team.Blu)
    {
        this.configZero = configZero;
        Size = new Vector2(220, 70);
        PositionCondition = ImGuiCond.FirstUseEver;
        Position = DefaultPosition;
    }

    public long? TimeRemaining { private get; set; }
    public long? MaxTime { private get; set; }
    
    public override void Draw()
    {
        if (TimeRemaining is null or 0)
        {
            MaxTime = null;
            if (!configZero.Timer.RepositionMode)
            {
                return;
            }
        }

        if (TimeRemaining is null && configZero.Timer.RepositionMode)
        {
            TimeRemaining = (33 * 60) + 30;
            MaxTime = (long)((float)TimeRemaining * 1.33f);
        }

        if (configZero.Timer.RepositionMode)
        {
            Flags &= ~ImGuiWindowFlags.NoMove;
        }
        else
        {
            Flags |= ImGuiWindowFlags.NoMove;
        }

        MaxTime ??= TimeRemaining;
        if (MaxTime < TimeRemaining) MaxTime = TimeRemaining;
        var text = $"{TimeRemaining / 60}:{(TimeRemaining % 60).ToString().PadLeft(2, '0')}";
        ImGui.PushFont(Tf2Font);
        var regionAvailable = ImGui.GetContentRegionAvail();
        var timerSize = ImGui.CalcTextSize(text);
        var circleCenter = ImGui.GetWindowPos() + new Vector2(185, 35);
        const int circleRadius = 25;
        ImGui.SetCursorPosX(((regionAvailable.X - (circleRadius * 2) - timerSize.X) / 2) + 10);
        ImGui.SetCursorPosY(((regionAvailable.Y - timerSize.Y) / 2) + 10);
        ImGui.Text(text);
        ImGui.PopFont();
        ImGui.SameLine();
        ImGui.GetForegroundDrawList()
             .AddCircleFilled(circleCenter, circleRadius, new Vector4(49 / 255f, 44 / 255f, 41 / 255f, 1f).ToU32());
        var startAngle = getAngleValue(0);
        var normalizedTimeRemaining = (float)((MaxTime - (MaxTime - TimeRemaining)) / (float)MaxTime);
        var color = (normalizedTimeRemaining > 0.1f ? Colors.White : Colors.Red).ToU32();
        ImGui.GetForegroundDrawList().PathArcTo(circleCenter, circleRadius, startAngle,
                                                getAngleValue(Math.Min(0.5f, normalizedTimeRemaining)));
        ImGui.GetForegroundDrawList().PathLineTo(circleCenter);
        ImGui.GetForegroundDrawList().PathFillConvex(color);
        if (normalizedTimeRemaining >= 0.5f)
        {
            ImGui.GetForegroundDrawList().PathArcTo(circleCenter, circleRadius, getAngleValue(0.5f),
                                                    getAngleValue(normalizedTimeRemaining));
            ImGui.GetForegroundDrawList().PathLineTo(circleCenter);
            ImGui.GetForegroundDrawList().PathFillConvex(color);
        }
    }


    private static float getAngleValue(float point)
    {
        return (float)(Math.PI * ((point * 360) - 90) / 180.0f);
    }
}
