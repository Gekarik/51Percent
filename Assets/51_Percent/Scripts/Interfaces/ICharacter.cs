using System;
using UnityEngine;

public interface ICharacter
{
    string Name { get; }
    void Init(ColorService colorService, TerritoryManager territoryManager, IHexGridProvider grid);
    bool HasActiveTrail { get; }
    PlayerStatsComponent StatsComponent { get; }
    CharacterState State { get; }
    Color Color { get; }
    Transform Transform { get; }
    Transform GetSocket(SocketType socket);
    event Action<ICharacter, ICharacter> TrailInterrupted;
    event Action<ICharacter> TrailOrphaned;
    void Kill();
    void Die();
}
