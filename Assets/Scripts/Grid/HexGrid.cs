using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
    [SerializeField] private ViewAnimator _viewAnimator;

    private List<Hex> _allHexes;

    private void Awake()
    {
        _allHexes = GetComponentsInChildren<Hex>(true).ToList();
    }

    public IReadOnlyList<Hex> AllHexes => _allHexes;

    public Hex GetRandomHex()
    {
        if (_allHexes.Count == 0)
            return null;

        int index = Random.Range(0, _allHexes.Count);
        return _allHexes[index];
    }

    public List<Hex> GetNeighbors(Hex startHex, float searchRadius)
    {
        var neighbors = new List<Hex>();

        LayerMask hexMask = 1 << startHex.gameObject.layer;

        var point = startHex.transform.position;
        var colliders = Physics.OverlapSphere(startHex.transform.position, searchRadius, hexMask);

        Debug.Log(colliders.Length); 

        foreach (var collider in colliders)
        {
            if (collider.TryGetComponent(out Hex hex))
                neighbors.Add(hex);
        }

        Debug.Log(neighbors.Count().ToString());

        return neighbors;
    }

    public void OnAreaCaptured(IReadOnlyList<Transform> hexesView)
    {
        _viewAnimator.Wave(hexesView);
    }
}
