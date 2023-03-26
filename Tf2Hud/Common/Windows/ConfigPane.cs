using KamiLib.Interfaces;
using Tf2Hud.Common.Configuration;

namespace Tf2Hud.Common.Windows;

public abstract class ConfigPane: ISelectable, IDrawable
{
    public abstract void Draw();

    public IDrawable Contents => this;
    public string ID => $"##{GetType()}";

    public abstract void DrawLabel();

}

public abstract class ConfigPane<T>: ConfigPane where T: ModuleConfiguration
{
    internal readonly T Config;

    protected ConfigPane(T config)
    {
        this.Config = config;
    }
}
