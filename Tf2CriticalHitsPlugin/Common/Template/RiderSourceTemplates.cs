using Dalamud.Logging;
using JetBrains.Annotations;

namespace Tf2CriticalHitsPlugin.Common.Template;

public static class RiderSourceTemplates
{
    [SourceTemplate]
    public static void log(this object obj)
    {
        PluginLog.Debug(obj.ToString());
    } 
}
