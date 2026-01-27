using UnityEngine;

[DefaultExecutionOrder(-100)]
public class Bootstrap : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private HexGrid _hexGrid;
    [SerializeField] private TerritoryManager _territoryManager;
    [SerializeField] private PlayerSpawner _playerSpawner;
    [SerializeField] private EnemySpawner _enemySpawner;

    [Header("Settings")]
    [SerializeField] private ColorSettings _colorSettings;

    private ColorService _colorService;
    private KillManager _killManager;

    private void Awake()
    {
        _colorService = new ColorService(_colorSettings);
        _killManager = new KillManager();

        _playerSpawner.Init(_hexGrid, _territoryManager, _killManager, _colorService);
        _enemySpawner.Init(_hexGrid, _territoryManager, _killManager, _colorService);
    }
}
