using UnityEngine;

public class BoosterSpawner : CollectibleSpawnerBase
{
    protected override CollectibleKind Kind => CollectibleKind.Booster;

    [Header("Boosters")]
    [Required] [SerializeField] private Booster[] _boosterPrefabs;
    [SerializeField] private Transform _container;

    protected override ICollectible Acquire()
    {
        var prefab = _boosterPrefabs[Random.Range(0, _boosterPrefabs.Length)];
        return Instantiate(prefab, _container);
    }

    protected override void Dispose(ICollectible item)
    {
        Destroy(item.Transform.gameObject);
    }
}
