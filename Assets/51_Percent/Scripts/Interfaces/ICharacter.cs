using System;
using System.Collections.Generic;
using UnityEngine;

public interface ICharacter
{
    string Name { get; }
    bool IsHuman { get; }
    void Init(ColorService colorService, TerritoryManager territoryManager, IHexGridProvider grid,
        KillManager killManager, IMatchState matchState);
    bool CanAct { get; }
    bool HasActiveTrail { get; }
    PlayerStats LifeStats { get; }
    CharacterState State { get; }
    Color Color { get; }
    Transform Transform { get; }
    Transform GetSocket(SocketType socket);
    event Action<ICharacter, ICharacter> TrailInterrupted;
    event Action<ICharacter> TrailOrphaned;
    event Action<ICharacter, IReadOnlyList<IHex>> AreaCaptured;
    IBoosterObservable BoosterObservable { get; }
    ITrailVisualProvider TrailVisual { get; }
    void Kill();
    bool TryDie();
}
