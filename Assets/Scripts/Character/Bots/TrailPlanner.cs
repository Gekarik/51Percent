using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrailPlanner
{
    public List<Hex> BuildTrail(IReadOnlyCollection<Hex> fixedCells, IHexGridProvider grid, int aggression)
    {
        if (fixedCells.Count == 0) return null;
        return aggression <= 1
            ? BuildRandomTrail(fixedCells, grid, 3)
            : BuildDistanceArcWithOffset(fixedCells, grid, aggression);
    }

    private List<Hex> BuildRandomTrail(IReadOnlyCollection<Hex> fixedCells, IHexGridProvider grid, int length)
    {
        var rnd = new System.Random();
        var start = fixedCells.ElementAt(rnd.Next(fixedCells.Count));
        var trail = new List<Hex> { start };
        for (int i = 0; i < length; i++)
        {
            var cand = grid.GetNeighbors(trail.Last())
                .Where(h => !fixedCells.Contains(h) && !trail.Contains(h)).ToList();
            if (!cand.Any()) break;
            trail.Add(cand[rnd.Next(cand.Count)]);
        }
        Debug.Log($"[TrailPlanner] Random trail: {trail.Count}");
        return trail.Count > 1 ? trail : null;
    }

    private List<Hex> BuildDistanceArcWithOffset(IReadOnlyCollection<Hex> fixedCells, IHexGridProvider grid, int aggression)
    {
        var rnd = new System.Random();
        var start = fixedCells.ElementAt(rnd.Next(fixedCells.Count));

        int minDist = aggression;
        int maxDist = aggression * 2;
        var candidates = grid.AllHexes
            .Where(h => !fixedCells.Contains(h))
            .Where(h => grid.Distance(start, h) >= minDist && grid.Distance(start, h) <= maxDist)
            .ToList();
        if (!candidates.Any())
        {
            Debug.Log($"[TrailPlanner] No targets in dist [{minDist},{maxDist}]");
            return null;
        }

        var target = candidates[rnd.Next(candidates.Count)];
        Debug.Log($"[TrailPlanner] Arc target at dist {grid.Distance(start, target)}");

        var forward = Pathfinder.AStar(start, target, grid, h => true);
        if (forward == null || forward.Count < 2)
        {
            Debug.Log("[TrailPlanner] AStar failed");
            return null;
        }

        // »спользуем заранее рассчитанный диаметр €чейки
        float cellDiameter = grid.CellDiameter;
        // Ўирина дуги зависит от агрессии
        float offsetDist = cellDiameter * (0.5f + aggression * 0.1f);

        int midIndex = forward.Count / 2;
        Vector3 midPos = forward[midIndex].transform.position;
        Vector3 dir = (target.transform.position - start.transform.position).normalized;
        Vector3 perp = Vector3.Cross(dir, Vector3.up).normalized * (rnd.Next(0, 2) == 0 ? 1 : -1) * offsetDist;
        Vector3 detourPos = midPos + perp;

        var detourHex = grid.AllHexes
            .OrderBy(h => Vector3.Distance(h.transform.position, detourPos))
            .First();
        Debug.Log($"[TrailPlanner] Detour hex at {detourHex.transform.position}");

        var path1 = Pathfinder.AStar(start, detourHex, grid, h => true);
        var path2 = Pathfinder.AStar(detourHex, target, grid, h => true);
        if (path1 == null || path2 == null)
            return forward;

        var result = new List<Hex>(path1);
        result.AddRange(path2.Skip(1));
        Debug.Log($"[TrailPlanner] Offset arc built: {result.Count}");
        return result;
    }

    public List<Hex> BuildReturn(List<Hex> trail, IReadOnlyCollection<Hex> fixedCells, IHexGridProvider grid)
    {
        if (trail == null || trail.Count < 2) return null;
        var path = Pathfinder.AStar(trail.Last(), trail.First(), grid, h => fixedCells.Contains(h) || h == trail.First());
        Debug.Log(path != null ? $"[TrailPlanner] Return: {path.Count}" : "[TrailPlanner] Return null");
        return path;
    }
}