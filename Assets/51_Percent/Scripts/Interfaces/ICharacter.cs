using UnityEngine;

public interface ICharacter
{
    string Name { get; }
    bool IsHuman { get; }
    bool CanAct { get; }
    PlayerStats LifeStats { get; }
    CharacterState State { get; }
    Color Color { get; }
    Transform Transform { get; }
    Transform GetSocket(SocketType socket);
    IBoosterObservable BoosterObservable { get; }
    ITrailVisualProvider TrailVisual { get; }
    ITrailObservable Trail { get; }
    void Kill();
    bool TryDie();
}
