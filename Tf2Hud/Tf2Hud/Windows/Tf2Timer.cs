using System;
using System.Numerics;
using Dalamud.Interface.Raii;
using ImGuiNET;
using KamiLib.Drawing;
using Tf2Hud.Common.Configuration;
using Tf2Hud.Common.Model;

namespace Tf2Hud.Tf2Hud.Windows;

public class Tf2Timer : Tf2Window
{
    private float CircleRadius => 25 * Scale;
    private readonly ConfigZero configZero;

    public Tf2Timer(ConfigZero configZero) : base("##Tf2Timer", Tf2Team.Blu)
    {
        this.configZero = configZero;
        PositionCondition = ImGuiCond.Always;
    }

    private Vector2 CircleCenter => ImGui.GetWindowPos() + ImGui.GetWindowSize() - new Vector2(35 * Scale, 35 * Scale);

    public long? TimeRemaining { private get; set; }
    public long? MaxTime { private get; set; }

    public override void Draw()
    {
        Size = new Vector2(220 * Scale, 70 * Scale);
        Position = configZero.Timer.GetPosition();
        if (TimeRemaining is null or 0)
        {
            MaxTime = null;
            if (!configZero.Timer.RepositionMode) return;
        }

        if (TimeRemaining is null && configZero.Timer.RepositionMode)
        {
            TimeRemaining = (33 * 60) + 30;
            MaxTime = (long)((float)TimeRemaining * 1.33f);
        }

        if (configZero.Timer.RepositionMode)
            Flags &= ~ImGuiWindowFlags.NoMove;
        else
            Flags |= ImGuiWindowFlags.NoMove;


        MaxTime ??= TimeRemaining;
        if (MaxTime is null || TimeRemaining is null) return;
        if (MaxTime < TimeRemaining) MaxTime = TimeRemaining;

        DrawTimerText();
        DrawTimerCircle();
    }

    private void DrawTimerText()
    {
        var text = $"{TimeRemaining / 60}:{(TimeRemaining % 60).ToString()?.PadLeft(2, '0')}";
        var regionAvailable = ImGui.GetContentRegionAvail();
        var timerSize = CalculateTextSize(Tf2Font, text, Scale / 2);
        ImGui.SetCursorPosX(((regionAvailable.X - (CircleRadius * 2) - timerSize.X) / 2) + 5);
        ImGui.SetCursorPosY(((regionAvailable.Y - timerSize.Y) / 2) + 10);
        // ImGui.TextColored(TanLight, text);
        ImGui.GetWindowDrawList().AddText(Tf2Font, Tf2Font.FontSize / 2 * Scale, ImGui.GetCursorScreenPos(), TanLight.ToU32(),  text);
    }
    
    private static Vector2 CalculateTextSize(ImFontPtr font, string text, float scale)
    {

        var textSize = ImGui.CalcTextSize(text);
        var fontScalar = font.FontSize / textSize.Y;

        var textWidth = textSize.X * fontScalar;

        return new Vector2(textWidth, font.FontSize) * scale;
    }


    private float Scale => configZero.Timer.Scale.Value;

    private void DrawTimerCircle()
    {
        if (MaxTime is null || TimeRemaining is null) return;
        var timerBackground = new Vector4(49 / 255f, 44 / 255f, 41 / 255f, 1f).ToU32();
        ImGui.GetWindowDrawList()
             .AddCircleFilled(CircleCenter, CircleRadius, timerBackground);
        var startAngle = GetAngleValue(0);
        var normalizedTimeRemaining = (float)((MaxTime - (MaxTime - TimeRemaining)) / (float)MaxTime);
        var color = (normalizedTimeRemaining > 0.25f ? TanLight : Colors.Red).ToU32();
        ImGui.GetWindowDrawList().PathArcTo(CircleCenter, CircleRadius, startAngle,
                                                GetAngleValue(Math.Min(0.5f, normalizedTimeRemaining)));
        ImGui.GetWindowDrawList().PathLineTo(CircleCenter);
        ImGui.GetWindowDrawList().PathFillConvex(color);
        if (normalizedTimeRemaining >= 0.5f)
        {
            ImGui.GetWindowDrawList().PathArcTo(CircleCenter, CircleRadius, GetAngleValue(0.5f),
                                                    GetAngleValue(normalizedTimeRemaining));
            ImGui.GetWindowDrawList().PathLineTo(CircleCenter);
            ImGui.GetWindowDrawList().PathFillConvex(color);
        }
    }


    private static float GetAngleValue(float point)
    {
        return (float)(Math.PI * ((point * 360) - 90) / 180.0f);
    }
}
