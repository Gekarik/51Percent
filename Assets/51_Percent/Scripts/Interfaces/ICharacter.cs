using System;
using UnityEngine;

public interface ICharacter
{
    string Name { get; }
    bool IsHuman { get; }
    void Init(ColorService colorService, TerritoryManager territoryManager, IHexGridProvider grid, KillManager killManager);
    bool HasActiveTrail { get; }
    PlayerStatsComponent StatsComponent { get; }
    CharacterState State { get; }
    Color Color { get; }
    Transform Transform { get; }
    Transform GetSocket(SocketType socket);
    event Action<ICharacter, ICharacter> TrailInterrupted;
    event Action<ICharacter> TrailOrphaned;
    event Action RespawnRequested;
    IBoosterObservable BoosterObservable { get; }
    ITrailVisualProvider TrailVisual { get; }
    void Kill();
    void Die();
}
