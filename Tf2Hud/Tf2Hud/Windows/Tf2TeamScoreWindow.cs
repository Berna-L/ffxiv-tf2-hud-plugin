using Tf2Hud.Common.Model;

namespace Tf2Hud.Tf2Hud.Windows;

public abstract class Tf2TeamScoreWindow : Tf2Window
{
    protected Tf2TeamScoreWindow(string name, Tf2Team team) : base(name, team) { }

    public uint Score { get; set; } = 0;
}
