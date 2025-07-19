using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Mover))]
public class PathProvider : VectorProviderComponent
{
    private readonly List<Vector3> _waypoints = new List<Vector3>();
    private int _current = 0;

    public void SetPath(List<Hex> hexPath, IHexGridProvider grid)
    {
        _waypoints.Clear();
        foreach (var h in hexPath)
            _waypoints.Add(h.transform.position);
        _current = 0;
        Debug.Log($"[PathProvider:{name}] Path set: {_waypoints.Count} points");
    }

    public override Vector3 GetMoveDirection()
    {
        if (_current >= _waypoints.Count)
            return Vector3.zero;

        Vector3 target = _waypoints[_current];
        Vector3 raw = target - transform.position;
        raw.y = 0;
        if (raw.magnitude < 0.2f)
        {
            _current++;
            return Vector3.zero;
        }
        return raw.normalized;
    }

    public bool IsDone => _current >= _waypoints.Count;
}
