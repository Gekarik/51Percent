using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Контекст с информацией о мире для принятия решений ботом
/// </summary>
public class BotContext
{
    // Ссылки на компоненты
    public Transform BotTransform;
    public IHexGridProvider Grid;
    public Conqueror Conqueror;
    public ICharacter Character;
    public BotPersonalitySettings Personality;

    // Текущее состояние
    public Vector3 Position => BotTransform.position;
    public int TrailLength => Conqueror?.TrailHexes?.Count ?? 0;
    public bool HasTrail => TrailLength > 0;
    public IReadOnlyCollection<IHex> Territory => Conqueror?.FixedHexes;
    public IReadOnlyList<IHex> Trail => Conqueror?.TrailHexes;
    public IHex CurrentHex;

    // Обнаруженные объекты (заполняются EnemyBrain)
    public List<Transform> NearbyCoins = new List<Transform>(8);
    public List<ICharacter> NearbyEnemies = new List<ICharacter>(4);
    public List<IHex> DangerousTrails = new List<IHex>(8);

    // Текущее направление движения (для инерции)
    public Vector3 CurrentDirection;

    // Вспомогательные методы
    public Vector3 GetTerritoryCenter()
    {
        var territory = Territory;
        if (territory == null || territory.Count == 0)
            return Position;

        Vector3 center = Vector3.zero;
        int count = 0;
        foreach (var hex in territory)
        {
            if (hex?.Transform != null)
            {
                center += hex.Transform.position;
                count++;
            }
        }
        return count > 0 ? center / count : Position;
    }

    public IHex GetNearestHomeHex()
    {
        var territory = Territory;
        if (territory == null || territory.Count == 0)
            return null;

        IHex nearest = null;
        float nearestDist = float.MaxValue;

        foreach (var hex in territory)
        {
            if (hex?.Transform == null) continue;

            float dist = Vector3.Distance(Position, hex.Transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = hex;
            }
        }
        return nearest;
    }

    public void Clear()
    {
        NearbyCoins.Clear();
        NearbyEnemies.Clear();
        DangerousTrails.Clear();
        CurrentHex = null;
    }
}
