using System.IO;
using Newtonsoft.Json;
using Tf2Hud.Configuration;

namespace Tf2Hud.Common.Configuration;

public class ConfigZero : BaseConfiguration
{
    public ConfigZero()
    {
        Version = 1;
    }

    public void Save()
    {
        PluginVersion = PluginVersion.Current;
        File.WriteAllText(Service.PluginInterface.ConfigFile.FullName,
                          JsonConvert.SerializeObject(this, Formatting.Indented));
    }
}
