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

    protected abstract CollectibleKind Kind { get; }

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
        item.Consumed += OnConsumed;
        item.Collected += OnCollected;
        _registry?.Register(item, Kind);
        _count++;
        return item;
    }

    private void OnConsumed(ICollectible item)
    {
        item.Consumed -= OnConsumed;
        _count--;
    }

    private void OnCollected(ICollectible item)
    {
        item.Collected -= OnCollected;
        Dispose(item);
    }
}
