namespace Tf2Hud.Tf2Hud.Windows;

public abstract class Tf2TeamScoreWindow: Tf2Window
{
    public int Score { get; set; } = 0;

    protected Tf2TeamScoreWindow(string name, Team team) : base(name, team)
    {
        
    }
}
