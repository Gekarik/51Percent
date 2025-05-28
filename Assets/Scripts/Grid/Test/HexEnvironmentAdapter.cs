using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HexEnvironmentAdapter : IHexEnvironment
{
    private readonly HexGrid _grid;
    public HexEnvironmentAdapter(HexGrid grid) => _grid = grid;
    public IReadOnlyList<Hex> AllHexes => _grid.AllHexes;
    public IEnumerable<Hex> GetNeighbors(Hex hex)
    {
        float radius = _grid.NeighborSearchRadius;
        return _grid.GetNeighborsCached(hex)
            .Where(n => Vector3.Distance(hex.transform.position,
                                         n.transform.position) <= radius);
    }
}
