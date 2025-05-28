using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HexGrid : MonoBehaviour, IHexGridProvider
{
    [SerializeField] private ViewAnimator _viewAnimator;

    private const float RadiusSearchFactor = 1.01f;
    private List<Hex> _allHexes;
    private Dictionary<Hex, List<Hex>> _neighborMap;
    private float _neighborSearchRadius;
    public float NeighborSearchRadius => _neighborSearchRadius;
    public IReadOnlyList<Hex> AllHexes => _allHexes;

    public ViewAnimator ViewAnimator => _viewAnimator;

    private void Awake()
    {
        _allHexes = GetComponentsInChildren<Hex>(true).ToList();
        float hexDiameter = _allHexes[0].GetRendererBounds().size.z;
        _neighborSearchRadius = hexDiameter * RadiusSearchFactor;
        BuildNeighborMap();
    }

    private void BuildNeighborMap()
    {
        _neighborMap = new Dictionary<Hex, List<Hex>>(_allHexes.Count);
        LayerMask hexMask = 1 << _allHexes[0].gameObject.layer;

        foreach (var hex in _allHexes)
        {
            var colliders = Physics.OverlapSphere(hex.transform.position, _neighborSearchRadius, hexMask);
            var neighbors = new List<Hex>();
            foreach (var collider in colliders)
            {
                if (collider.TryGetComponent(out Hex h) && h != hex)
                    neighbors.Add(h);
            }

            _neighborMap[hex] = neighbors;
        }
    }

    public IReadOnlyList<Hex> GetNeighborsCached(Hex hex) => _neighborMap.TryGetValue(hex, out var n) ? n : new List<Hex>();

    public IEnumerable<Hex> GetNeighbors(Hex hex)
    {
        float radius = NeighborSearchRadius;
        return GetNeighborsCached(hex)
            .Where(n => Vector3.Distance(hex.transform.position,
                                         n.transform.position) <= radius);
    }

    public Hex GetRandomHex()
    {
        if (_allHexes.Count == 0)
            return null;

        int index = Random.Range(0, _allHexes.Count);
        return _allHexes[index];
    }

    public void OnAreaCaptured(IReadOnlyCollection<Transform> hexesView)
    {
        if (_viewAnimator != null)
        {
            _viewAnimator.Wave(hexesView);
            Debug.Log(hexesView.Count);
        }
    }
}
