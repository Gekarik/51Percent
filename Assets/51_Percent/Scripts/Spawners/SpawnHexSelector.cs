using System;
using System.Collections.Generic;
using UnityEngine;

// Выбирает клетку появления персонажа, когда заранее заданные точки спавна исчерпаны.
// Порядок критериев именно такой: сначала честность (появление не отбирает чужую территорию),
// и лишь среди подходящих клеток — удобство, то есть удалённость от живых участников.
// Единым числовым счётом это не выражается: иначе достаточно далёкая чужая клетка
// обыгрывала бы свободную, а смерть превращалась бы в способ отрезать чужие владения.
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
            // Чужой трейл — это мгновенное столкновение сразу после появления
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

        // Свободного места может не остаться к концу матча: тогда важнее не появиться
        // под носом у соперника, чем сохранить нетронутой его территорию
        return bestFree ?? bestOccupied ?? _grid.GetRandomHex();
    }

    // Стартовая территория выдаётся на клетку и её соседей, поэтому свободной считается
    // вся эта область целиком, а не одна клетка
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
