using System.Collections.Generic;
using UnityEngine;

// Ориентиры бота по собственной территории: куда возвращаться и где её середина.
// Самостоятельные вычисления над позициями — проверяются без сцены и клеток.
// Наблюдения собирает Unity-адаптер (EnemyBrain), он же решает, когда их запрашивать.
public class TerritoryBearings
{
    // Если территории нет, ориентиром остаётся сам бот: двигаться ему некуда
    public Vector3 NearestPoint(Vector3 from, IReadOnlyList<Vector3> territory)
    {
        if (territory == null || territory.Count == 0)
            return from;

        Vector3 nearest = from;
        float nearestSqr = float.MaxValue;

        for (int i = 0; i < territory.Count; i++)
        {
            float distanceSqr = (from - territory[i]).sqrMagnitude;

            if (distanceSqr >= nearestSqr)
                continue;

            nearestSqr = distanceSqr;
            nearest = territory[i];
        }

        return nearest;
    }

    public Vector3 Center(Vector3 fallback, IReadOnlyList<Vector3> territory)
    {
        if (territory == null || territory.Count == 0)
            return fallback;

        Vector3 sum = Vector3.zero;

        for (int i = 0; i < territory.Count; i++)
            sum += territory[i];

        return sum / territory.Count;
    }
}
