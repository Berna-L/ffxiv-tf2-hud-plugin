﻿using System.Numerics;

namespace Tf2Hud.Tf2Hud;

public class Team
{
    public static readonly Team Blu = new("BLU")
    {
        BgColor = new Vector4(88 / 255f, 133 / 255f, 162 / 255f, 1f),
        TextColor = new Vector4(153 / 255f, 204 / 255f, 255 / 255f, 1f),
    };

    public static readonly Team Red = new("RED")
    {
        BgColor = new Vector4(184 / 255f, 56 / 255f, 59 / 255f, 1f),
        TextColor = new Vector4(255 / 255f, 64 / 255f, 64 / 255f, 1f),
    };

    public Team(string name)
    {
        Name = name;
    }


    public string Name { get; private init; }
    public Vector4 BgColor { get; private init; }
    public Vector4 TextColor { get; private init; }
    public Team Enemy => IsBlu ? Red : Blu;
    public bool IsBlu => Name == Blu.Name;
    public bool IsRed => !IsBlu;

}
