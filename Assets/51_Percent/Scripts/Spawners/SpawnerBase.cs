using UnityEngine;

public abstract class SpawnerBase : MonoBehaviour
{
    [SerializeField] protected BoxCollider _spawnArea;
    [SerializeField] protected float _verticalOffset = 1f;
    [SerializeField] protected float _borderInset = 0f;

    protected Vector3 GetRandomPosition()
    {
        Bounds bounds = _spawnArea.bounds;
        float x = Random.Range(bounds.min.x + _borderInset, bounds.max.x - _borderInset);
        float z = Random.Range(bounds.min.z + _borderInset, bounds.max.z - _borderInset);
        return new Vector3(x, bounds.center.y + _verticalOffset, z);
    }
}
