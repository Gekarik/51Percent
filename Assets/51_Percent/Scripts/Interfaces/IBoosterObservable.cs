using System;

public interface IBoosterObservable
{
    IBoosterEffect ActiveEffect { get; }
    bool HasActiveBooster { get; }
    float RemainingTime { get; }
    IBoosterEffect PendingEffect { get; }
    bool HasPendingBooster { get; }
    event Action BoosterChanged;
}
