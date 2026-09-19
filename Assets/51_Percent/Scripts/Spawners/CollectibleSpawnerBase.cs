using System.Collections;
using UnityEngine;

public abstract class CollectibleSpawnerBase : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private SpawnPointProvider _spawnPoint;
    [SerializeField] private float _initialDelay = 0f;
    [SerializeField] private float _spawnInterval = 1f;
    [SerializeField] private int _maxObjects = 25;

    private ICollectibleRegistry _registry;
    private int _count;
    private Coroutine _spawnRoutine;

    public void SetRegistry(ICollectibleRegistry registry)
    {
        _registry = registry;
    }

    // Наследник статически знает, какой род коллектиблов он спавнит
    protected abstract CollectibleKind Kind { get; }

    // Источник и утилизация — единственное, что варьируется между спавнерами
    protected abstract ICollectible Acquire();
    protected abstract void Dispose(ICollectible item);

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
        if (_initialDelay > 0f)
            yield return new WaitForSeconds(_initialDelay);

        var wait = new WaitForSeconds(_spawnInterval);

        while (true)
        {
            if (_count < _maxObjects)
                SpawnAt(_spawnPoint.GetRandomPosition());

            yield return wait;
        }
    }

    protected ICollectible SpawnAt(Vector3 position)
    {
        var item = Acquire();
        item.Transform.position = position;
        item.Collected += OnCollected;
        _registry?.Register(item, Kind);
        _count++;
        return item;
    }

    private void OnCollected(ICollectible item)
    {
        item.Collected -= OnCollected;
        Dispose(item);
        _count--;
    }
}
