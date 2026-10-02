using System;

public interface IBoosterLifecycle
{
    event Action<BoosterId> BoosterStarted;
    event Action<BoosterId, BoosterEndReason> BoosterEnded;
}
