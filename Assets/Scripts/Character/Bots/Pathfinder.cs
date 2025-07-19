
using System;
using System.Collections.Generic;
using UnityEngine;

public static class Pathfinder
{
    public static List<Hex> AStar(
        Hex start,
        Hex goal,
        IHexGridProvider grid,
        Func<Hex, bool> canEnter)
    {
        var openSet = new SimplePriorityQueue<Hex>();
        openSet.Enqueue(start, 0);

        var cameFrom = new Dictionary<Hex, Hex>();
        var gScore = new Dictionary<Hex, float> { { start, 0 } };
        var fScore = new Dictionary<Hex, float> { { start, Heuristic(start, goal) } };

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();
            if (current == goal) return ReconstructPath(cameFrom, current);

            foreach (var neighbor in grid.GetNeighbors(current))
            {
                if (!canEnter(neighbor)) continue;
                float tentativeG = gScore[current] + 1f;
                if (!gScore.TryGetValue(neighbor, out var oldG) || tentativeG < oldG)
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    float f = tentativeG + Heuristic(neighbor, goal);
                    fScore[neighbor] = f;
                    openSet.EnqueueOrUpdate(neighbor, f);
                }
            }
        }
        return new List<Hex>();
    }

    static float Heuristic(Hex a, Hex b)
        => Vector3.Distance(a.transform.position, b.transform.position);

    static List<Hex> ReconstructPath(Dictionary<Hex, Hex> cameFrom, Hex current)
    {
        var path = new List<Hex> { current };
        while (cameFrom.TryGetValue(current, out var prev))
        {
            current = prev;
            path.Add(current);
        }
        path.Reverse();
        return path;
    }
}

class SimplePriorityQueue<T>
{
    readonly List<(T item, float priority)> _data = new();
    public int Count => _data.Count;
    public void Enqueue(T item, float priority) => _data.Add((item, priority));
    public void EnqueueOrUpdate(T item, float priority)
    {
        for (int i = 0; i < _data.Count; i++)
            if (EqualityComparer<T>.Default.Equals(_data[i].item, item))
            {
                _data[i] = (item, priority);
                return;
            }
        Enqueue(item, priority);
    }
    public T Dequeue()
    {
        _data.Sort((a, b) => a.priority.CompareTo(b.priority));
        var best = _data[0];
        _data.RemoveAt(0);
        return best.item;
    }
}
