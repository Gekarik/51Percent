using System.Collections.Generic;
using UnityEngine;

public class TerritoryBearings
{
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
