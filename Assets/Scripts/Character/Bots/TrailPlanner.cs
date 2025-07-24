using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Формирует дугу захвата (trail) и путь возврата на основе fixedCells и параметра агрессии.
/// </summary>
public class TrailPlanner
{
    /// <summary>
    /// Построить основной трейл: случайная дуга или возврат пусто.
    /// </summary>
    public List<Hex> BuildTrail(IReadOnlyCollection<Hex> fixedCells, IHexGridProvider grid, int aggression)
    {
        // Если нет фиксированных клеток, ничего не делаем.
        if (fixedCells.Count == 0) return null;

        // Для низкой агрессии строим короткую случайную дугу
        if (aggression <= 1)
            return BuildRandomTrail(fixedCells, grid, 3);

        // Иначе строим дугу по параметру агрессии
        return BuildArcTrail(fixedCells, grid, aggression);
    }

    private List<Hex> BuildRandomTrail(IReadOnlyCollection<Hex> fixedCells, IHexGridProvider grid, int length)
    {
        var rnd = new System.Random();
        var start = fixedCells.ElementAt(rnd.Next(fixedCells.Count));
        var trail = new List<Hex> { start };

        for (int i = 0; i < length; i++)
        {
            var neighbors = grid.GetNeighbors(trail.Last())
                .Where(h => !fixedCells.Contains(h) && !trail.Contains(h))
                .ToList();
            if (!neighbors.Any()) break;
            trail.Add(neighbors[rnd.Next(neighbors.Count)]);
        }

        return trail.Count > 1 ? trail : null;
    }

    private List<Hex> BuildArcTrail(IReadOnlyCollection<Hex> fixedCells, IHexGridProvider grid, int aggression)
    {
        var rnd = new System.Random();
        var start = fixedCells.ElementAt(rnd.Next(fixedCells.Count));

        int minDist = aggression;
        int maxDist = aggression * 2;
        var targets = grid.AllHexes
            .Where(h => !fixedCells.Contains(h))
            .Where(h => grid.Distance(start, h) >= minDist && grid.Distance(start, h) <= maxDist)
            .ToList();
        if (!targets.Any()) return null;

        var target = targets[rnd.Next(targets.Count)];
        var direct = Pathfinder.AStar(start, target, grid, h => !fixedCells.Contains(h));
        if (direct == null || direct.Count < 3) return null;

        // Разбиваем путь на три сегмента
        int count = direct.Count;
        int idx1 = count / 3;
        int idx2 = 2 * count / 3;
        Vector3 pos1 = direct[idx1].transform.position;
        Vector3 pos2 = direct[idx2].transform.position;

        Vector3 dir = (target.transform.position - start.transform.position).normalized;
        Vector3 perp = Vector3.Cross(dir, Vector3.up).normalized;
        float offsetBase = grid.CellDiameter * aggression * 0.3f;

        // Два детур-пойнта с разными смещениями
        Vector3 det1 = pos1 + perp * offsetBase;
        Vector3 det2 = pos2 - perp * offsetBase;

        var hex1 = grid.AllHexes.OrderBy(h => Vector3.Distance(h.transform.position, det1)).First();
        var hex2 = grid.AllHexes.OrderBy(h => Vector3.Distance(h.transform.position, det2)).First();

        // Пути между ключевыми точками
        var seg1 = Pathfinder.AStar(start, hex1, grid, h => !fixedCells.Contains(h));
        var seg2 = Pathfinder.AStar(hex1, hex2, grid, h => !fixedCells.Contains(h));
        var seg3 = Pathfinder.AStar(hex2, target, grid, h => !fixedCells.Contains(h));
        if (seg1 == null || seg2 == null || seg3 == null) return direct;

        var arc = new List<Hex>(seg1);
        arc.AddRange(seg2.Skip(1));
        arc.AddRange(seg3.Skip(1));
        return arc.Count > 1 ? arc : null;
    }

    /// <summary>
    /// Построение пути возврата по фиксированным клеткам.
    /// </summary>
    public List<Hex> BuildReturn(List<Hex> trail, IReadOnlyCollection<Hex> fixedCells, IHexGridProvider grid)
    {
        if (trail == null || trail.Count < 2) return null;
        var from = trail.Last();
        var to = trail.First();
        var path = Pathfinder.AStar(from, to, grid, h => fixedCells.Contains(h) || h == to);
        return path != null && path.Count > 1 ? path : null;
    }
}
