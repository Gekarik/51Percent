using System;
using System.Collections.Generic;

// Наблюдение за забегом персонажа. Отделён от ICharacter намеренно: события трейла нужны
// только правилам убийства, волне захвата и оценке целей бота, а остальным потребителям
// персонажа — нет. Имя по образцу IBoosterObservable: владелец состояния трейла — TrailRun,
// здесь только наблюдение за ним
public interface ITrailObservable
{
    bool HasActiveTrail { get; }
    event Action<ICharacter, ICharacter> TrailInterrupted;
    event Action<ICharacter> TrailOrphaned;
    event Action<ICharacter, IReadOnlyList<IHex>> AreaCaptured;
}
