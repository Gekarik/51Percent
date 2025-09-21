using System.Collections.Generic;
using System.Linq;

public class ConquestAlgorithm: IConquestAlgorithm
{
    public List<IHex> ComputeCapturedArea(IReadOnlyCollection<IHex> fixedHexes,
        IReadOnlyCollection<Hex> trailHexes,
        IHexGridProvider hexGridProvider)
    {
        var barrier = new HashSet<IHex>(fixedHexes);
        barrier.UnionWith(trailHexes);

        var allHexes = hexGridProvider.AllHexes;
        int count = allHexes.Count;
        var neighborMap = new Dictionary<IHex, List<IHex>>(count);
        var neighborCounts = new Dictionary<IHex, int>(count);

        foreach (var h in allHexes)
        {
            var neighs = hexGridProvider.GetNeighbors(h).ToList();
            neighborMap[h] = neighs;
            neighborCounts[h] = neighs.Count;
        }

        int maxNeighbors = neighborCounts.Values.DefaultIfEmpty(0).Max();

        var borderSeeds = new Queue<IHex>();
        var visited = new HashSet<IHex>();
        foreach (var h in allHexes)
        {
            if (neighborCounts[h] < maxNeighbors && !barrier.Contains(h))
            {
                borderSeeds.Enqueue(h);
                visited.Add(h);
            }
        }

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

        var toCapture = new List<IHex>(count);
        
        foreach (var h in allHexes)
        {
            if (!barrier.Contains(h) && !visited.Contains(h))
                toCapture.Add(h);
        }
        
        toCapture.AddRange(trailHexes);
        return toCapture;
    }
}