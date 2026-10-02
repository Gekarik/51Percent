using System.Collections.Generic;
using UnityEngine;

public class TrailThreatEvaluator
{
    private const int WatchedSegmentLength = 5;

    public bool IsTrailThreatened(Vector3 botPosition, IReadOnlyList<Vector3> trailPositions,
        IReadOnlyList<Vector3> rivalPositions, float detectionRadius, float threatRadius)
    {
        if (trailPositions == null || trailPositions.Count == 0 || rivalPositions == null)
            return false;

        float detectionSqr = detectionRadius * detectionRadius;
        float threatSqr = threatRadius * threatRadius;
        int from = Mathf.Max(0, trailPositions.Count - WatchedSegmentLength);

        for (int r = 0; r < rivalPositions.Count; r++)
        {
            Vector3 rival = rivalPositions[r];

            if ((rival - botPosition).sqrMagnitude > detectionSqr)
                continue;

            for (int i = from; i < trailPositions.Count; i++)
                if ((rival - trailPositions[i]).sqrMagnitude < threatSqr)
                    return true;
        }

        return false;
    }
}
