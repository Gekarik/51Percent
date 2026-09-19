using System;

public interface IMatchState
{
    bool IsRunning { get; }
    bool IsPaused { get; }
    bool IsFinished { get; }
    event Action Changed;
}
