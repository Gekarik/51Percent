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

    [Header("UI")]
    [SerializeField] private Transform _uiRoot;
    [SerializeField] private LeaderBoardView _leaderBoardView;

    private ColorService _colorService;
    private KillManager _killManager;
    private LeaderBoardModel _leaderBoardModel;

    private void Awake()
    {
        _colorService = new ColorService(_colorSettings);
        _leaderBoardModel = new LeaderBoardModel(_territoryManager);
        _killManager = new KillManager(_leaderBoardModel);

        _playerSpawner.Init(_hexGrid, _territoryManager, _killManager, _colorService, _leaderBoardModel);
        _playerSpawner.SetUIRoot(_uiRoot);
        _enemySpawner.Init(_hexGrid, _territoryManager, _killManager, _colorService, _leaderBoardModel);

        _leaderBoardView.Init(_leaderBoardModel);
    }

    private void OnDestroy()
    {
        _leaderBoardModel?.Dispose();
    }
}
