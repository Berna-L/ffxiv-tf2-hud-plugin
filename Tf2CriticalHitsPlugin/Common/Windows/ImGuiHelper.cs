using System.Numerics;
using ImGuiNET;
using KamiLib.Drawing;

namespace Tf2CriticalHitsPlugin.Common.Windows;

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

    public static void ForegroundTextShadow(ImFontPtr font, string text, Vector2 position)
    {
        ImGui.GetForegroundDrawList().AddText(font, 100.0f, position + DefaultShadowOffset, Colors.Black.ToU32(), text);
        ImGui.GetForegroundDrawList().AddText(font, 100.0f, position, Colors.White.ToU32(), text);
    }
    
    public static Vector2 CalcTextSize(ImFontPtr fontPtr, string text)
    {
        ImGui.PushFont(fontPtr);
        var calcTextSize = ImGui.CalcTextSize(text);
        ImGui.PopFont();
        return calcTextSize;
    }

}
