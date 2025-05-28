using System.Collections.Generic;
using UnityEngine;

public interface IHexGridProvider
{
    ViewAnimator ViewAnimator { get; }
    void OnAreaCaptured(IReadOnlyCollection<Transform> hexesView);
    IReadOnlyList<Hex> AllHexes { get; }
    IEnumerable<Hex> GetNeighbors(Hex hex);
}
