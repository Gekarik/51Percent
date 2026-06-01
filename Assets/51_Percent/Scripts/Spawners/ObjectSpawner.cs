using System;
using System.Collections;
using UnityEngine;

public abstract class ObjectSpawner<T> : MonoBehaviour where T : MonoBehaviour, ICollectible
{
    [Header("Spawn Settings")]
    [SerializeField] private T _spawnPrefab;
    [SerializeField] private Transform _container;
    [SerializeField] private SpawnPointProvider _spawnPoint;
    [SerializeField] private float _spawnInterval = 1f;
    [SerializeField] private int _maxObjects = 25;

    private int _counter = 0;
    private WaitForSeconds _wait;

    private ICollectibleRegistry _registry;
    private ObjectPool<T> _pool;
    private Coroutine _spawnRoutine;
    
    public void SetRegistry(ICollectibleRegistry registry)
    {
        _registry = registry;
    }

    private void Awake()
    {
        if (_spawnPrefab == null)
            throw new InvalidOperationException("Spawn prefab is not assigned");
        
        _wait = new WaitForSeconds(_spawnInterval);

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
            
            if(_counter < _maxObjects)
                SpawnOnce();
        }
    }

    private void SpawnOnce()
    {
        SpawnAtPosition(_spawnPoint.GetRandomPosition());
    }

    protected T SpawnAtPosition(Vector3 position)
    {
        T item = _pool.Get();
        item.transform.position = position;
        item.Collected += OnItemCollected;
        _registry?.Register(item);
        _counter++;
        return item;
    }

    private void OnItemCollected(ICollectible collectible)
    {
        T item = collectible as T;
        collectible.Collected -= OnItemCollected;
        _pool.Release(item);
        _counter--;
    }

}
