using System.Collections.Generic;
using System.Linq;

public class ConquestAlgorithm : IConquestAlgorithm
{
    private const int MaxNeighborCount = 6;

    public List<IHex> ComputeCapturedArea(
        IReadOnlyCollection<IHex> fixedHexes,
        IReadOnlyCollection<IHex> trailHexes,
        IHexGridProvider hexGridProvider)
    {
        HashSet<IHex> barrier = new HashSet<IHex>(fixedHexes);
        barrier.UnionWith(trailHexes);

        IReadOnlyList<IHex> allHexes = hexGridProvider.AllHexes;
        int hexCount = allHexes.Count;

        Dictionary<IHex, List<IHex>> neighborMap = new Dictionary<IHex, List<IHex>>(hexCount);
        Queue<IHex> borderSeeds = new Queue<IHex>();
        HashSet<IHex> visited = new HashSet<IHex>();

        foreach (IHex hex in allHexes)
        {
            List<IHex> neighbors = hexGridProvider.GetNeighbors(hex).ToList();
            neighborMap[hex] = neighbors;

            bool isBorderHex = neighbors.Count < MaxNeighborCount;

            if (isBorderHex && !barrier.Contains(hex))
            {
                borderSeeds.Enqueue(hex);
                visited.Add(hex);
            }
        }

        while (borderSeeds.Count > 0)
        {
            IHex currentHex = borderSeeds.Dequeue();

            foreach (IHex neighbor in neighborMap[currentHex])
            {
                if (barrier.Contains(neighbor) || visited.Contains(neighbor))
                    continue;

                visited.Add(neighbor);
                borderSeeds.Enqueue(neighbor);
            }
        }

        List<IHex> capturedHexes = new List<IHex>(hexCount);

        foreach (IHex hex in allHexes)
        {
            if (!barrier.Contains(hex) && !visited.Contains(hex))
                capturedHexes.Add(hex);
        }

        capturedHexes.AddRange(trailHexes);

        return capturedHexes;
    }
}
