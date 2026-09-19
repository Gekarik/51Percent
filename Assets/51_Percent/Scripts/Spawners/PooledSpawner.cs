using System;
using UnityEngine;

public abstract class PooledSpawner<T> : CollectibleSpawnerBase where T : MonoBehaviour, ICollectible
{
    [Header("Pool")]
    [Required] [SerializeField] private T _prefab;
    [SerializeField] private Transform _container;

    private ObjectPool<T> _pool;

    private void Awake()
    {
        if (_prefab == null)
            throw new InvalidOperationException("Spawn prefab is not assigned");

        _pool = new ObjectPool<T>(_prefab, _container != null ? _container : transform);
    }

    protected override ICollectible Acquire() => _pool.Get();

    protected override void Dispose(ICollectible item) => _pool.Release((T)item);

    // Типизированный спавн для наследников, которым нужен конкретный T (напр. рассыпание монет)
    protected T SpawnTyped(Vector3 position) => (T)SpawnAt(position);
}
