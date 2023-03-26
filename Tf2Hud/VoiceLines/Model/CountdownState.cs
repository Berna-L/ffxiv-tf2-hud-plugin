using System;

namespace Tf2Hud.VoiceLines.Model;

public class CountdownState
{
    private static readonly CountdownState instance = new();
    public bool countdownCancelled;

    private bool countingDown;

    private bool inCombat;

    private CountdownState() { }

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
    public int StartingValue { get; private set; }
    public float CountDownValue { get; set; }
    public bool PrePulling { get; set; } = false;

    public static CountdownState Instance()
    {
        return instance;
    }

    public event EventHandler? InCombatChanged;
    public event EventHandler? CountingDownChanged;
    public event EventHandler? StartCountingDown;
    public event EventHandler? StopCountingDown;

    public void FireStartCountingDown()
    {
        StartingValue = (int)Math.Round(CountDownValue);
        StartCountingDown?.Invoke(this, EventArgs.Empty);
    }

    public void FireStopCountingDown(bool cancelled)
    {
        countdownCancelled = cancelled;
        StopCountingDown?.Invoke(this, EventArgs.Empty);
    }
}
