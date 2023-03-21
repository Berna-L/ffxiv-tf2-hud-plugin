using Dalamud.Logging;
using JetBrains.Annotations;

namespace Tf2Hud.Common.Template;

public static class RiderSourceTemplates
{
    [SourceTemplate]
    public static void log(this object obj)
    {
        PluginLog.Debug(obj.ToString());
    }
}
