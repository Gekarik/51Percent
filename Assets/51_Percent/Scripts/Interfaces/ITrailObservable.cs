using System;
using System.Collections.Generic;

public interface ITrailObservable
{
    bool HasActiveTrail { get; }
    event Action<ICharacter, ICharacter> TrailInterrupted;
    event Action<ICharacter> TrailOrphaned;
    event Action<ICharacter, IReadOnlyList<IHex>> AreaCaptured;
}
