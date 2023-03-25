using KamiLib.Interfaces;
using Tf2Hud.Common.Configuration;

namespace Tf2Hud.Common.Windows;

public abstract class ConfigPane : ISelectable, IDrawable
{
    internal readonly ConfigZero configZero;

    protected ConfigPane(ConfigZero configZero)
    {
        this.configZero = configZero;
    }

    public abstract void Draw();

    public IDrawable Contents => this;
    public string ID => $"##{GetType()}";

    public abstract void DrawLabel();
}
