using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(PolygonCollider2D))]
public class PolygonZoneTester : MonoBehaviour
{
    private PolygonCollider2D _polygonCollider;
    private Vector2[] polygonPoints = new Vector2[0];

    private void Awake()
    {
        _polygonCollider = GetComponent<PolygonCollider2D>();
        _polygonCollider.isTrigger = true;
    }

    public void SetPolygon(IList<Vector2> worldPoints)
    {
        // Локальные точки относительно transform позиции
        Vector2[] localPoints = new Vector2[worldPoints.Count];
        Vector2 origin = transform.position;
        for (int i = 0; i < worldPoints.Count; i++)
            localPoints[i] = worldPoints[i] - origin;

        _polygonCollider.pathCount = 1;
        _polygonCollider.SetPath(0, localPoints);
    }


    public bool IsInside(Vector2 point) => _polygonCollider.OverlapPoint(point);
    private void OnDrawGizmos()
    {
        if (_polygonCollider == null || _polygonCollider.pathCount == 0)
            return;

        Gizmos.color = Color.red;
        Vector2[] points = _polygonCollider.GetPath(0);

        for (int i = 0; i < points.Length; i++)
        {
            Vector2 start = transform.TransformPoint(points[i]);
            Vector2 end = transform.TransformPoint(points[(i + 1) % points.Length]);

            Gizmos.DrawLine(start, end);
        }
    }

}