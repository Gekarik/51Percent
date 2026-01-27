using UnityEngine;

public interface ICharacter
{
    void Init(ColorService colorService, TerritoryManager territoryManager, IHexGridProvider grid);
    Conqueror Conqueror { get; }
    CharacterState State { get; }
    Color Color { get; }
    Transform Transform { get; }
    void Kill();
    void Die();
}
