using System.Collections.Generic;

public interface IHexGridProvider
{
    IReadOnlyList<Hex> AllHexes { get; }
    float CellDiameter { get; }

    IEnumerable<IHex> GetNeighbors(IHex hex);
    int Distance(IHex a, Hex b);
}
