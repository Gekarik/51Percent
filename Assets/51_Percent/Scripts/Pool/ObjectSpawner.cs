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

    private void Awake()
    {
        if (_spawnPrefab == null)
            throw new InvalidOperationException("Spawn prefab is not assigned");

        _pool = new ObjectPool<T>(_spawnPrefab, _container ?? transform);
    }

    private void OnEnable()
    {
        _spawnRoutine = StartCoroutine(SpawnLoop());
    }

    private void OnDisable()
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
        T item = _pool.Get();
        item.transform.position = GetRandomPosition();

        item.Collected += OnItemCollected;
    }

    private void OnItemCollected(IGrabbable grabbable)
    {
        T item = grabbable as T;
        grabbable.Collected -= OnItemCollected;
        _pool.Release(item);
    }

    private Vector3 GetRandomPosition()
    {
        Bounds bounds = _spawnArea.bounds;
        float x = UnityEngine.Random.Range(bounds.min.x, bounds.max.x);
        float z = UnityEngine.Random.Range(bounds.min.z, bounds.max.z);

        return new Vector3(x, bounds.center.y + _verticalOffset, z);
    }
}
