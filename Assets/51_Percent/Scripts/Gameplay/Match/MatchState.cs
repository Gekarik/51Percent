using System;

public class MatchState : IMatchState
{
    public MatchPhase Phase { get; private set; } = MatchPhase.Running;
    public bool IsRunning => Phase == MatchPhase.Running;
    public bool IsPaused => Phase == MatchPhase.Paused;
    public bool IsFinished => Phase == MatchPhase.Finished;
    public event Action Changed;

    public bool TryPause()
    {
        if (!IsRunning)
            return false;

        SetPhase(MatchPhase.Paused);
        return true;
    }

    public bool TryResume()
    {
        if (!IsPaused)
            return false;

        SetPhase(MatchPhase.Running);
        return true;
    }

    public bool TryFinish()
    {
        if (IsFinished)
            return false;

        SetPhase(MatchPhase.Finished);
        return true;
    }

    private void SetPhase(MatchPhase phase)
    {
        Phase = phase;
        Changed?.Invoke();
    }
}
