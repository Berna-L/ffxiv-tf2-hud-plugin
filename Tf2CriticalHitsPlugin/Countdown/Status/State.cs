using System;

namespace Tf2CriticalHitsPlugin.Countdown.Status;

public class State
{

    private static State instance = new();
    
    public static State Instance()
    {
        return instance;
    }

    private State()
    {
        
    }
    
    private bool countingDown;

    private bool inCombat;
    public bool countdownCancelled = false;
    public TimeSpan CombatDuration { get; set; } = new(0);
    public DateTime CombatEnd { get; set; } = DateTime.UnixEpoch;
    public DateTime CombatStart { get; set; } = DateTime.UnixEpoch;
    public bool Mocked { get; set; }

    public bool InCombat
    {
        get => inCombat;
        set
        {
            if (inCombat == value) return;
            inCombat = value;
            InCombatChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool CountingDown
    {
        get => countingDown;
        set
        {
            if (countingDown == value) return;
            countingDown = value;
            CountingDownChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool InInstance { get; set; }
    public float CountDownValue { get; set; }
    public bool PrePulling { get; set; } = false;
    public event EventHandler? InCombatChanged;
    public event EventHandler? CountingDownChanged;
    public event EventHandler? StartCountingDown;
    public event EventHandler? StopCountingDown;

    public void FireStartCountingDown()
    {
        StartCountingDown?.Invoke(this, EventArgs.Empty);
    }

    public void FireStopCountingDown(bool cancelled)
    {
        countdownCancelled = cancelled;
        StopCountingDown?.Invoke(this, EventArgs.Empty);
    }
}
