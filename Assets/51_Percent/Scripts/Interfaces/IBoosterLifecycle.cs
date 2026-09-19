using System;

// Жизненный цикл активного бустера. Отделён от IBoosterObservable намеренно:
// HUD читает состояние и не нуждается в событиях, а презентация пропов
// нуждается в событиях и не нуждается в остатке времени
public interface IBoosterLifecycle
{
    event Action<BoosterId> BoosterStarted;
    event Action<BoosterId, BoosterEndReason> BoosterEnded;
}
