using UnityEngine;

public class RespawnShieldBuilder
{
    private const string ShieldName = "RespawnShield";
    private const string SegmentName = "Segment";
    private const int SegmentCount = 12;
    private const float SegmentThickness = 0.3f;
    private const float OverlapFactor = 1.2f;

    public GameObject Build(Vector3 center, float radius, float height)
    {
        var root = new GameObject(ShieldName);
        root.transform.position = center;

        float segmentWidth = 2f * Mathf.PI * radius / SegmentCount * OverlapFactor;
        float angleStep = 360f / SegmentCount;

        for (int i = 0; i < SegmentCount; i++)
        {
            var segment = new GameObject(SegmentName);
            segment.transform.SetParent(root.transform, false);

            Quaternion rotation = Quaternion.Euler(0f, i * angleStep, 0f);
            segment.transform.localRotation = rotation;
            segment.transform.localPosition = rotation * Vector3.forward * radius + Vector3.up * (height * 0.5f);

            var box = segment.AddComponent<BoxCollider>();
            box.size = new Vector3(segmentWidth, height, SegmentThickness);
        }

        return root;
    }
}
