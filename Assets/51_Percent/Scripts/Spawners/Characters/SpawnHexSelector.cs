using System;
using System.Collections.Generic;
using UnityEngine;

public class SpawnHexSelector
{
    private readonly IHexGridProvider _grid;
    private readonly IReadOnlyList<ICharacter> _characters;

    public SpawnHexSelector(IHexGridProvider grid, IReadOnlyList<ICharacter> characters)
    {
        _grid = grid ?? throw new ArgumentNullException(nameof(grid));
        _characters = characters;
    }

    public IHex SelectSpawnHex()
    {
        IHex bestFree = null;
        float bestFreeDistance = float.NegativeInfinity;
        IHex bestOccupied = null;
        float bestOccupiedDistance = float.NegativeInfinity;

        foreach (IHex hex in _grid.AllHexes)
        {
            if (hex.State == HexState.PartOfTrail)
                continue;

            float distance = DistanceToNearestCharacter(hex);

            if (IsAreaFree(hex))
            {
                if (distance <= bestFreeDistance)
                    continue;

                bestFreeDistance = distance;
                bestFree = hex;
            }
            else if (distance > bestOccupiedDistance)
            {
                bestOccupiedDistance = distance;
                bestOccupied = hex;
            }
        }

        return bestFree ?? bestOccupied ?? _grid.GetRandomHex();
    }

    private bool IsAreaFree(IHex hex)
    {
        if (hex.Owner != null)
            return false;

        foreach (IHex neighbor in _grid.GetNeighbors(hex))
            if (neighbor.Owner != null)
                return false;

        return true;
    }

    private float DistanceToNearestCharacter(IHex hex)
    {
        if (_characters == null)
            return float.MaxValue;

        Vector3 hexPosition = hex.Transform.position;
        float nearest = float.MaxValue;

        for (int i = 0; i < _characters.Count; i++)
        {
            ICharacter character = _characters[i];

            if (character == null || character.State != CharacterState.Alive)
                continue;

            Vector3 offset = character.Transform.position - hexPosition;
            float distance = offset.x * offset.x + offset.z * offset.z;

            if (distance < nearest)
                nearest = distance;
        }

        return nearest;
    }
}
