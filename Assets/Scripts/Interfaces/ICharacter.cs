using UnityEngine;

public interface ICharacter
{
    void InitConquester(IHexGridProvider hexGrid);
    Conquester Conquester { get; }
    CharacterState State { get; }
    Color Color { get; }
    Transform Transform { get; }
    void Kill();
    void Die();
}
