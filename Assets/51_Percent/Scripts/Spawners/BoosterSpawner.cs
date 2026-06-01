using System.Collections;
using UnityEngine;

public class BoosterSpawner : MonoBehaviour
{
    [SerializeField] private Booster[] _boosterPrefabs;
    [SerializeField] private Transform _container;
    [SerializeField] private SpawnPointProvider _spawnPoint;
    [SerializeField] private float _initialDelay = 10f;
    [SerializeField] private float _spawnInterval = 60f;
    [SerializeField] private int _maxBoosters = 3;

    private int _activeCount;
    private ICollectibleRegistry _registry;
    private Coroutine _spawnRoutine;

    public void SetRegistry(ICollectibleRegistry registry)
    {
        _registry = registry;
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
        yield return new WaitForSeconds(_initialDelay);

        while (true)
        {
            if (_activeCount < _maxBoosters)
                SpawnRandom();
            yield return new WaitForSeconds(_spawnInterval);
        }
    }

    private void SpawnRandom()
    {
        var prefab = _boosterPrefabs[Random.Range(0, _boosterPrefabs.Length)];
        var booster = Instantiate(prefab, _spawnPoint.GetRandomPosition(), Quaternion.identity, _container);
        booster.Collected += OnBoosterCollected;
        _registry?.Register(booster);
        _activeCount++;
    }

    private void OnBoosterCollected(ICollectible collectible)
    {
        collectible.Collected -= OnBoosterCollected;
        _registry?.Unregister(collectible);
        _activeCount--;
        Destroy(collectible.Transform.gameObject);
    }

}
