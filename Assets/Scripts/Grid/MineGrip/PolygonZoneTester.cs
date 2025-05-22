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

    public void SetPolygon(IList<Vector2> points)
    {
        int count = points.Count;

        if (polygonPoints.Length < count)
            polygonPoints = new Vector2[count];

        for (int i = 0; i < count; i++)
            polygonPoints[i] = points[i];

        _polygonCollider.pathCount = 1;
        _polygonCollider.SetPath(0, polygonPoints);
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