using System.Collections.Generic;
using UnityEngine;

public interface IHexGridProvider
{
    ViewAnimator ViewAnimator { get; }
    void OnAreaCaptured(IReadOnlyCollection<Transform> hexesView);
    IReadOnlyList<Hex> AllHexes { get; }
    float CellDiameter { get; }

    IEnumerable<Hex> GetNeighbors(Hex hex);
    int Distance(Hex a, Hex b);
}
