using UnityEngine;

public interface ICharacter
{
    CharacterState State { get; }
    Color Color { get; }
    void Kill();
    void Die();
}