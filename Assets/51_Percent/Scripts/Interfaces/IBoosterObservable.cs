using System;

public interface IBoosterObservable
{
    IBoosterEffect ActiveEffect { get; }
    bool HasActiveBooster { get; }
    float RemainingTime { get; }
    event Action BoosterChanged;
}
