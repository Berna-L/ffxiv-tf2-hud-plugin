using Tf2Hud.Tf2Hud.Model;

namespace Tf2Hud.Tf2Hud.Windows;

public abstract class Tf2TeamScoreWindow: Tf2Window
{
    public int Score { get; set; } = 0;

    protected Tf2TeamScoreWindow(string name, Tf2Team team) : base(name, team)
    {
        
    }
}
