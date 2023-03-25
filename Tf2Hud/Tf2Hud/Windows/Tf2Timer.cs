using System;
using System.Numerics;
using Dalamud.Interface.Raii;
using ImGuiNET;
using KamiLib.Drawing;
using Tf2Hud.Common.Configuration;

namespace Tf2Hud.Tf2Hud.Windows;

public class Tf2Timer : Tf2Window
{
    private const int CircleRadius = 25;
    private readonly ConfigZero configZero;
    private static Vector2 CircleCenter => ImGui.GetWindowPos() + new Vector2(185, 35);

    public Tf2Timer(ConfigZero configZero) : base("##Tf2Timer", Team.Blu)
    {
        this.configZero = configZero;
        Size = new Vector2(220, 70);
        PositionCondition = ImGuiCond.Always;
    }

    public long? TimeRemaining { private get; set; }
    public long? MaxTime { private get; set; }
    
    public override void Draw()
    {
        Service.Log($"Tf2Timer - Starting to Draw");
        Position = configZero.Timer.GetPosition();
        if (TimeRemaining is null or 0)
        {
            Service.Log($"Tf2Timer - TimeRemaining is null or 0");

            MaxTime = null;
            if (!configZero.Timer.RepositionMode)
            {
                Service.Log($"Tf2Timer - and it's in reposition mode");
                return;
            }
        }

        if (TimeRemaining is null && configZero.Timer.RepositionMode)
        {
            Service.Log($"Tf2Timer - TimeRemaining is null, but it's reposition mode: setting fake time");
            TimeRemaining = (33 * 60) + 30;
            MaxTime = (long)((float)TimeRemaining * 1.33f);
        }

        if (configZero.Timer.RepositionMode)
        {
            Service.Log($"Tf2Timer - It's reposition mode!");
            Flags &= ~ImGuiWindowFlags.NoMove;
        }
        else
        {
            Service.Log($"Tf2Timer - It's not reposition mode :(");
            Flags |= ImGuiWindowFlags.NoMove;
        }


        MaxTime ??= TimeRemaining;
        if (MaxTime is null || TimeRemaining is null)
        {
            Service.Log($"Tf2Timer - MaxTime is {MaxTime} and TimeRemaining is {TimeRemaining}. One of them is null, and thus it returneth.");
            return;
        }
        if (MaxTime < TimeRemaining)
        {
            Service.Log($"Tf2Timer - MaxTime < TimeRemaining. Updating MaxTime.");
            MaxTime = TimeRemaining;
        }
        
        Service.Log($"Tf2Timer - Everything OK to starting creating the window itself.");

        DrawTimerText();
        ImGui.SameLine();
        DrawTimerCircle();
    }

    private void DrawTimerText()
    {
        using (ImRaii.PushFont(Tf2Font))
        {
            var text = $"{TimeRemaining / 60}:{(TimeRemaining % 60).ToString()?.PadLeft(2, '0')}";
            var regionAvailable = ImGui.GetContentRegionAvail();
            var timerSize = ImGui.CalcTextSize(text);
            ImGui.SetCursorPosX(((regionAvailable.X - (CircleRadius * 2) - timerSize.X) / 2) + 10);
            ImGui.SetCursorPosY(((regionAvailable.Y - timerSize.Y) / 2) + 10);
            ImGui.Text(text);
        }
    }

    private void DrawTimerCircle()
    {
        ImGui.GetForegroundDrawList()
             .AddCircleFilled(CircleCenter, CircleRadius, new Vector4(49 / 255f, 44 / 255f, 41 / 255f, 1f).ToU32());
        var startAngle = GetAngleValue(0);
        var normalizedTimeRemaining = (float)((MaxTime - (MaxTime - TimeRemaining)) / (float)MaxTime);
        var color = (normalizedTimeRemaining > 0.1f ? Colors.White : Colors.Red).ToU32();
        ImGui.GetForegroundDrawList().PathArcTo(CircleCenter, CircleRadius, startAngle,
                                                GetAngleValue(Math.Min(0.5f, normalizedTimeRemaining)));
        ImGui.GetForegroundDrawList().PathLineTo(CircleCenter);
        ImGui.GetForegroundDrawList().PathFillConvex(color);
        if (normalizedTimeRemaining >= 0.5f)
        {
            ImGui.GetForegroundDrawList().PathArcTo(CircleCenter, CircleRadius, GetAngleValue(0.5f),
                                                    GetAngleValue(normalizedTimeRemaining));
            ImGui.GetForegroundDrawList().PathLineTo(CircleCenter);
            ImGui.GetForegroundDrawList().PathFillConvex(color);
        }
    }


    private static float GetAngleValue(float point)
    {
        return (float)(Math.PI * ((point * 360) - 90) / 180.0f);
    }
}
