using System;
using System.IO;

namespace Tf2Hud.Common;

public class BernaUtil
{

    public enum OS
    {
        WINDOWS,
        MACOS,
        LINUX
    }

    public static OS GetOS()
    {
        if (!Dalamud.Utility.Util.IsLinux())
        {
            return OS.WINDOWS;
        }

        if (Path.Exists("Z:/Users/"))
        {
            return OS.MACOS;
        }

        return OS.LINUX;
    }
    
}
