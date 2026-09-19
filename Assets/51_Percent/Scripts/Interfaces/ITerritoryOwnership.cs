using System;

public interface ITerritoryOwnership
{
    event Action OwnershipChanged;
    float GetOwnershipPercent(ICharacter character);
}
