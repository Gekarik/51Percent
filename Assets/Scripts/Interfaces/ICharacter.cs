using UnityEngine;

public interface ICharacter
{
    void InitConquester(IHexGridProvider hexGrid);
    Conquester Conquester { get; }
    CharacterState State { get; }
    Color Color { get; }
    void Kill();
    void Die();
}
