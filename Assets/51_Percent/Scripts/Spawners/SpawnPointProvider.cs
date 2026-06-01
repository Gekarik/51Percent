using System;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class SpawnPointProvider
{
    [SerializeField] private BoxCollider _area;
    [SerializeField] private float _verticalOffset = 1f;
    [SerializeField] private float _borderInset = 0f;

    public Vector3 GetRandomPosition()
    {
        Bounds bounds = _area.bounds;
        float x = Random.Range(bounds.min.x + _borderInset, bounds.max.x - _borderInset);
        float z = Random.Range(bounds.min.z + _borderInset, bounds.max.z - _borderInset);
        return new Vector3(x, bounds.center.y + _verticalOffset, z);
    }
}
