using System;
using System.Reflection;

namespace Tf2Hud.Configuration;

public class PluginVersion : IComparable<PluginVersion>
{
    public static readonly PluginVersion Current;

    static PluginVersion()
    {
        var fullVersionText = Assembly.GetExecutingAssembly().FullName!.Split(',')[1];
        var version = fullVersionText[(fullVersionText.IndexOf('=') + 1)..].Split(".");
        Current = From(int.Parse(version[0]), int.Parse(version[1]), int.Parse(version[2]));
    }

    public int Major { get; init; }
    public int Minor { get; init; }
    public int Patch { get; init; }

    public int CompareTo(PluginVersion? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        var majorComparison = Major.CompareTo(other.Major);
        if (majorComparison != 0) return majorComparison;
        var minorComparison = Minor.CompareTo(other.Minor);
        if (minorComparison != 0) return minorComparison;
        return Patch.CompareTo(other.Patch);
    }

    public static PluginVersion From(int major, int minor, int patch)
    {
        return new PluginVersion
        {
            Major = major,
            Minor = minor,
            Patch = patch
        };
    }

    public bool Before(int major, int minor, int patch)
    {
        return CompareTo(From(major, minor, patch)) < 0;
    }
}
