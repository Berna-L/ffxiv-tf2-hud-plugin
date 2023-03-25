using System.IO;

namespace Tf2Hud.Common;

public class BernaUtil
{
    public enum OperatingSystem
    {
        Windows,
        Macos,
        Linux
    }

    public static OperatingSystem GetOperatingSystem()
    {
        if (!Dalamud.Utility.Util.IsLinux()) return OperatingSystem.Windows;
        return Path.Exists("Z:/Users/") ? OperatingSystem.Macos : OperatingSystem.Linux;
    }
}
