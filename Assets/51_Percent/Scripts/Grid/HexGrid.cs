using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-50)]
public class HexGrid : MonoBehaviour, IHexGridProvider
{
    private const float RowHeightMultiplier = 0.75f;
    private const float HalfOffset = 0.5f;

    [Required] [SerializeField] private HexNeighborOffsets _neighborOffsets;

    private List<Hex> _allHexes;
    private List<IHex> _allHexesAsInterface;
    private Dictionary<HexCoord, Hex> _coordToHex;

    private Hex _anchor;
    private float _hexWidth;
    private float _rowStep;

    public int Count => _allHexes.Count;
    public IReadOnlyList<IHex> AllHexes => _allHexesAsInterface;

    private void Awake()
    {
        CollectHexes();
        BuildCoordinateSystem();
        CalibrateSpacing();
    }

    private void CollectHexes()
    {
        _allHexes = GetComponentsInChildren<Hex>(true).ToList();

        if (_allHexes.Count == 0)
            throw new InvalidOperationException("HexGrid: no Hex children found");

        _allHexesAsInterface = _allHexes.Cast<IHex>().ToList();
    }

    private void BuildCoordinateSystem()
    {
        _coordToHex = new Dictionary<HexCoord, Hex>(_allHexes.Count);

        foreach (Hex hex in _allHexes)
        {
            if (_coordToHex.ContainsKey(hex.Coord))
                throw new InvalidOperationException(
                    $"HexGrid: duplicate coord {hex.Coord} on '{hex.name}'. " +
                    "Похоже, гексы без назначенных координат — перегенерируйте грид (Tools → 51 Percent → Grid Wizard)");

            _coordToHex[hex.Coord] = hex;
        }

        _anchor = _allHexes[0];
    }

    private void CalibrateSpacing()
    {
        Hex farInRow = null;
        Hex farInColumn = null;

        foreach (Hex hex in _allHexes)
        {
            if (hex.Coord.R == _anchor.Coord.R)
            {
                if (farInRow == null || DeltaQ(hex) > DeltaQ(farInRow))
                    farInRow = hex;
            }
            else if (farInColumn == null || DeltaR(hex) > DeltaR(farInColumn))
            {
                farInColumn = hex;
            }
        }

        Bounds fallbackBounds = _anchor.GetRendererBounds();

        _hexWidth = farInRow != null && DeltaQ(farInRow) > 0
            ? (farInRow.transform.position.x - _anchor.transform.position.x) / (farInRow.Coord.Q - _anchor.Coord.Q)
            : fallbackBounds.size.x;

        _rowStep = farInColumn != null
            ? (farInColumn.transform.position.z - _anchor.transform.position.z) / (farInColumn.Coord.R - _anchor.Coord.R)
            : fallbackBounds.size.z * RowHeightMultiplier;
    }

    private int DeltaQ(Hex hex) => Math.Abs(hex.Coord.Q - _anchor.Coord.Q);
    private int DeltaR(Hex hex) => Math.Abs(hex.Coord.R - _anchor.Coord.R);

    private HexCoord WorldToCoord(Vector3 worldPosition)
    {
        Vector3 anchorPosition = _anchor.transform.position;

        int row = _anchor.Coord.R + Mathf.RoundToInt((worldPosition.z - anchorPosition.z) / _rowStep);
        float rowShift = RowShift(row) - RowShift(_anchor.Coord.R);
        int column = _anchor.Coord.Q + Mathf.RoundToInt((worldPosition.x - anchorPosition.x - rowShift) / _hexWidth);

        return new HexCoord(column, row);
    }

    private float RowShift(int row) => (row & 1) == 1 ? _hexWidth * HalfOffset : 0f;

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
        HexCoord approximateCoord = WorldToCoord(worldPosition);
        IHex nearest = GetHex(approximateCoord);
        float nearestDistance = nearest != null ? PlanarDistanceSqr(nearest, worldPosition) : float.MaxValue;

        foreach (IHex neighbor in GetNeighbors(approximateCoord))
        {
            float distance = PlanarDistanceSqr(neighbor, worldPosition);

            if (distance >= nearestDistance)
                continue;

            nearestDistance = distance;
            nearest = neighbor;
        }

        return nearest;
    }

    private float PlanarDistanceSqr(IHex hex, Vector3 worldPosition)
    {
        Vector3 center = hex.Transform.position;
        float deltaX = center.x - worldPosition.x;
        float deltaZ = center.z - worldPosition.z;
        return deltaX * deltaX + deltaZ * deltaZ;
    }

    public HexCoord GetCoord(IHex hex)
    {
        return hex.Coord;
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
