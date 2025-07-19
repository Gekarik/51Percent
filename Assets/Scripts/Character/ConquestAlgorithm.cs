using System.Collections.Generic;
using System.Linq;

public class ConquestAlgorithm: IConquestAlgorithm
{
    public List<Hex> ComputeCapturedArea(IReadOnlyCollection<Hex> FixedHexes, IReadOnlyCollection<Hex> TrailHexes, IHexGridProvider hexGridProvider)
    {
        // 1) Формируем барьер
        var barrier = new HashSet<Hex>(FixedHexes);
        barrier.UnionWith(TrailHexes);

        // 2) Кешируем списки соседей и их размеры
        var allHexes = hexGridProvider.AllHexes;
        int count = allHexes.Count;
        var neighborMap = new Dictionary<Hex, List<Hex>>(count);
        var neighborCounts = new Dictionary<Hex, int>(count);

        foreach (var h in allHexes)
        {
            var neighs = hexGridProvider.GetNeighbors(h).ToList();
            neighborMap[h] = neighs;
            neighborCounts[h] = neighs.Count;
        }

        // 3) Находим максимальное число соседей
        int maxNeighbors = neighborCounts.Values.DefaultIfEmpty(0).Max();

        // 4) Определяем граничные узлы
        var borderSeeds = new Queue<Hex>();
        var visited = new HashSet<Hex>();
        foreach (var h in allHexes)
        {
            if (neighborCounts[h] < maxNeighbors && !barrier.Contains(h))
            {
                borderSeeds.Enqueue(h);
                visited.Add(h);
            }
        }

        // 5) BFS от всех borderSeeds
        while (borderSeeds.Count > 0)
        {
            var current = borderSeeds.Dequeue();
            foreach (var n in neighborMap[current])
            {
                if (barrier.Contains(n) || visited.Contains(n))
                    continue;
                visited.Add(n);
                borderSeeds.Enqueue(n);
            }
        }

        // 6) Составляем результат
        var toCapture = new List<Hex>(count);
        foreach (var h in allHexes)
        {
            if (!barrier.Contains(h) && !visited.Contains(h))
                toCapture.Add(h);
        }
        // Включаем трейл
        toCapture.AddRange(TrailHexes);
        return toCapture;
    }
}