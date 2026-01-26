using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HexGrid : MonoBehaviour, IHexGridProvider
{
    private const float RowHeightMultiplier = 0.75f;
    private const float HalfOffset = 0.5f;

    [SerializeField] private HexNeighborOffsets _neighborOffsets;

    private List<Hex> _allHexes;
    private List<IHex> _allHexesAsInterface;
    private Dictionary<HexCoord, Hex> _coordToHex;
    private float _hexWidth;
    private float _hexHeight;
    private Vector3 _gridOrigin;

    public int Count => _allHexes.Count;
    public IReadOnlyList<IHex> AllHexes => _allHexesAsInterface;

    private void Awake()
    {
        CollectHexes();
        CalculateGridParameters();
        BuildCoordinateSystem();
    }

    private void CollectHexes()
    {
        _allHexes = GetComponentsInChildren<Hex>(true).ToList();

        if (_allHexes.Count == 0)
            throw new InvalidOperationException("HexGrid: no Hex children found");

        _allHexesAsInterface = _allHexes.Cast<IHex>().ToList();
    }

    private void CalculateGridParameters()
    {
        Bounds firstHexBounds = _allHexes[0].GetRendererBounds();
        _hexWidth = firstHexBounds.size.x;
        _hexHeight = firstHexBounds.size.z;

        float minimumX = float.MaxValue;
        float minimumZ = float.MaxValue;

        foreach (Hex hex in _allHexes)
        {
            Vector3 position = hex.transform.position;

            if (position.x < minimumX)
                minimumX = position.x;

            if (position.z < minimumZ)
                minimumZ = position.z;
        }

        _gridOrigin = new Vector3(
            minimumX - _hexWidth * HalfOffset,
            0f,
            minimumZ - _hexHeight * HalfOffset);
    }

    private void BuildCoordinateSystem()
    {
        int hexCount = _allHexes.Count;
        _coordToHex = new Dictionary<HexCoord, Hex>(hexCount);

        foreach (Hex hex in _allHexes)
        {
            HexCoord coord = WorldToCoord(hex.transform.position);
            _coordToHex[coord] = hex;
        }
    }

    private HexCoord WorldToCoord(Vector3 worldPosition)
    {
        float relativeX = worldPosition.x - _gridOrigin.x;
        float relativeZ = worldPosition.z - _gridOrigin.z;

        int row = Mathf.RoundToInt(relativeZ / (_hexHeight * RowHeightMultiplier));
        bool isOddRow = (row & 1) == 1;
        float rowOffset = isOddRow ? _hexWidth * HalfOffset : 0f;
        int column = Mathf.RoundToInt((relativeX - rowOffset) / _hexWidth);

        return new HexCoord(column, row);
    }

    public Vector3 CoordToWorld(HexCoord coord)
    {
        float rowOffset = coord.IsOddRow ? _hexWidth * HalfOffset : 0f;
        float x = _gridOrigin.x + coord.Q * _hexWidth + rowOffset + _hexWidth * HalfOffset;
        float z = _gridOrigin.z + coord.R * _hexHeight * RowHeightMultiplier + _hexHeight * HalfOffset;

        return new Vector3(x, 0f, z);
    }

    public IHex GetHex(HexCoord coord)
    {
        _coordToHex.TryGetValue(coord, out Hex hex);
        return hex;
    }

    public bool TryGetHex(HexCoord coord, out IHex hex)
    {
        bool found = _coordToHex.TryGetValue(coord, out Hex concreteHex);
        hex = concreteHex;
        return found;
    }

    public IHex GetHexAt(Vector3 worldPosition)
    {
        HexCoord coord = WorldToCoord(worldPosition);
        return GetHex(coord);
    }

    public HexCoord GetCoord(IHex hex)
    {
        return WorldToCoord(hex.Transform.position);
    }

    public IEnumerable<IHex> GetNeighbors(HexCoord coord)
    {
        for (int i = 0; i < _neighborOffsets.Count; i++)
        {
            HexCoord offset = _neighborOffsets.GetOffset(i, coord.IsOddRow);
            HexCoord neighborCoord = new HexCoord(coord.Q + offset.Q, coord.R + offset.R);

            if (TryGetHex(neighborCoord, out IHex neighbor))
                yield return neighbor;
        }
    }

    public IEnumerable<IHex> GetNeighbors(IHex hex)
    {
        return GetNeighbors(GetCoord(hex));
    }

    public int CalculateDistance(HexCoord a, HexCoord b)
    {
        int deltaX = Math.Abs(a.CubeX - b.CubeX);
        int deltaY = Math.Abs(a.CubeY - b.CubeY);
        int deltaZ = Math.Abs(a.CubeZ - b.CubeZ);

        return (deltaX + deltaY + deltaZ) / 2;
    }

    public int CalculateDistance(IHex a, IHex b)
    {
        return CalculateDistance(GetCoord(a), GetCoord(b));
    }

    public IHex GetRandomHex()
    {
        int randomIndex = UnityEngine.Random.Range(0, _allHexes.Count);
        return _allHexes[randomIndex];
    }

    public IEnumerable<IHex> GetHexesInRadius(HexCoord center, int radius)
    {
        for (int deltaQ = -radius; deltaQ <= radius; deltaQ++)
        {
            for (int deltaR = -radius; deltaR <= radius; deltaR++)
            {
                HexCoord coord = new HexCoord(center.Q + deltaQ, center.R + deltaR);
                int distance = CalculateDistance(center, coord);

                if (distance <= radius && TryGetHex(coord, out IHex hex))
                    yield return hex;
            }
        }
    }
}
