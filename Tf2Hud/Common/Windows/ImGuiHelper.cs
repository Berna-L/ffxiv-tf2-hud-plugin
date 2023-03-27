using System.Numerics;
using Dalamud.Interface.Raii;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ImGuiNET;
using KamiLib.Drawing;

namespace Tf2Hud.Common.Windows;

public static class ImGuiHelper
{
    private static readonly Vector4 DefaultShadowColor = Colors.Black;
    private static readonly Vector2 DefaultShadowOffset = new(2, 2);

    public static void TextShadow(string text)
    {
        TextShadow(text, DefaultShadowColor, DefaultShadowOffset);
    }

    public static void TextShadow(string text, Vector4 shadowColor, Vector2 shadowOffset)
    {
        var cursorPos = ImGui.GetCursorPos();
        ImGui.SetCursorPos(cursorPos + shadowOffset);
        ImGui.TextColored(shadowColor, text);
        ImGui.SetCursorPos(cursorPos);
        ImGui.Text(text);
    }

    public static void ForegroundTextShadow(string id, ImFontPtr font, string text, Vector2 position)
    {
        var calcTextSize = CalcTextSize(font, text);
        ImGui.SetNextWindowSizeConstraints(calcTextSize + new Vector2(20, 20), calcTextSize + new Vector2(20, 20));
        ImGui.SetNextWindowBgAlpha(0f);
        ImGui.SetNextWindowPos(position - new Vector2(20, 20));
        using var borderRemove = ImRaii.PushStyle(ImGuiStyleVar.WindowBorderSize, 0f);
        ImGui.Begin($"{id}##window", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar |
                                  ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMouseInputs);
        ImGui.GetWindowDrawList().AddText(font, 100.0f, position + DefaultShadowOffset, Colors.Black.ToU32(), text);
        ImGui.GetWindowDrawList().AddText(font, 100.0f, position, Colors.White.ToU32(), text);
        ImGui.End();
    }

    public static Vector2 CalcTextSize(ImFontPtr fontPtr, string text)
    {
        ImGui.PushFont(fontPtr);
        var calcTextSize = ImGui.CalcTextSize(text);
        ImGui.PopFont();
        return calcTextSize;
    }
}
