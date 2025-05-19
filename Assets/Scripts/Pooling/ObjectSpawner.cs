using System;
using System.Collections;
using UnityEngine;

public abstract class ObjectSpawner<T> : MonoBehaviour where T : MonoBehaviour, IGrabbable
{
    [Header("Spawn Settings")]
    [SerializeField] private T _spawnPrefab;
    [SerializeField] private Transform _container;
    [SerializeField] private BoxCollider _spawnArea;
    [SerializeField] private float _verticalOffset = 1f;
    [SerializeField] private float _spawnInterval = 1f;

    private ObjectPool<T> _pool;
    private Coroutine _spawnRoutine;

    protected void Awake()
    {
        if (_spawnPrefab == null)
            throw new InvalidOperationException("Spawn prefab is not assigned");

        _pool = new ObjectPool<T>(_spawnPrefab, _container ?? transform);

    }

    protected virtual void OnEnable()
    {
        _spawnRoutine = StartCoroutine(SpawnLoop());
    }

    protected virtual void OnDisable()
    {
        if (_spawnRoutine != null)
            StopCoroutine(_spawnRoutine);
    }

    private IEnumerator SpawnLoop()
    {
        var wait = new WaitForSeconds(_spawnInterval);

        while (true)
        {
            yield return wait;
            SpawnOnce();
        }
    }

    private void SpawnOnce()
    {
        T instance = _pool.Get();
        instance.transform.position = GetRandomPosition();

        instance.Collected -= () => OnItemCollected(instance);
        instance.Collected += () => OnItemCollected(instance);
    }

    protected virtual void OnItemCollected(T item)
    {
        item.Collected -= () => OnItemCollected(item);
        _pool.Release(item);
    }

    protected Vector3 GetRandomPosition()
    {
        Bounds bounds = _spawnArea.bounds;
        float x = UnityEngine.Random.Range(bounds.min.x, bounds.max.x);
        float z = UnityEngine.Random.Range(bounds.min.z, bounds.max.z);
        return new Vector3(x, bounds.center.y + _verticalOffset, z);
    }
}