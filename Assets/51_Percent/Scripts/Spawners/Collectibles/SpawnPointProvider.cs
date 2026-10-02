using System;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class SpawnPointProvider
{
    [Required] [SerializeField] private BoxCollider _area;
    [Required] [SerializeField] private SpawnHeightSettings _height;
    [SerializeField] private float _borderInset = 0f;

    public Vector3 GetRandomPosition()
    {
        Bounds bounds = _area.bounds;
        float x = Random.Range(bounds.min.x + _borderInset, bounds.max.x - _borderInset);
        float z = Random.Range(bounds.min.z + _borderInset, bounds.max.z - _borderInset);
        return new Vector3(x, _height.WorldHeight, z);
    }
}
