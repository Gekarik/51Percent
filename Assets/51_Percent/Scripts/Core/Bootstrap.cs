using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class Bootstrap : MonoBehaviour
{
    [Header("Scene References")]
    [Required] [SerializeField] private HexGrid _hexGrid;
    [Required] [SerializeField] private TerritoryManager _territoryManager;
    [Required] [SerializeField] private PlayerSpawner _playerSpawner;
    [Required] [SerializeField] private EnemySpawner _enemySpawner;
    [Required] [SerializeField] private GameManager _gameManager;
    [Required] [SerializeField] private CoinSpawner _coinSpawner;
    [Required] [SerializeField] private BoosterSpawner _boosterSpawner;
    [Required] [SerializeField] private PlayerRespawner _playerRespawner;

    [Header("Settings")]
    [Required] [SerializeField] private ColorSettings _colorSettings;

    [Header("UI")]
    [Required] [SerializeField] private Transform _uiRoot;
    [Required] [SerializeField] private LeaderBoardView _leaderBoardView;
    [Required] [SerializeField] private CrownController _crownController;

    [Header("Dev (только на dev-сцене, опционально)")]
    [SerializeField] private DevTools _devTools;

    private ColorService _colorService;
    private KillManager _killManager;
    private LeaderBoardModel _leaderBoardModel;
    private WinConditionTracker _winConditionTracker;
    private CaptureWavePresenter _captureWavePresenter;
    private MatchState _matchState;

    private void Awake()
    {
        _matchState = new MatchState();
        _colorService = new ColorService(_colorSettings);
        _leaderBoardModel = new LeaderBoardModel(_territoryManager);
        _winConditionTracker = new WinConditionTracker(_territoryManager, _playerRespawner, _matchState);
        var elimination = new CharacterElimination(_territoryManager, _playerRespawner,
            _winConditionTracker, _leaderBoardModel, _coinSpawner);
        _killManager = new KillManager(_matchState, elimination);
        _captureWavePresenter = new CaptureWavePresenter(
            new TransformWaver(new WaveDelayResolver(WaveDirection.FromOrigin)));

        var allCharacters = new List<ICharacter>();
        var collectibleRegistry = new CollectibleRegistry();

        _playerRespawner.Init(_playerSpawner, _matchState);

        _playerSpawner.SetCharacterList(allCharacters);
        _playerSpawner.Init(_hexGrid, _territoryManager, _killManager, _colorService,
            _leaderBoardModel, _winConditionTracker, _matchState);
        _playerSpawner.SetUIRoot(_uiRoot);
        _playerSpawner.CharacterSpawned += OnCharacterSpawned;

        _enemySpawner.SetCharacterList(allCharacters);
        _enemySpawner.SetAIReferences(allCharacters, collectibleRegistry);
        _enemySpawner.Init(_hexGrid, _territoryManager, _killManager, _colorService,
            _leaderBoardModel, _winConditionTracker, _matchState);
        _enemySpawner.CharacterSpawned += OnCharacterSpawned;

        _coinSpawner.SetRegistry(collectibleRegistry);
        _boosterSpawner.SetRegistry(collectibleRegistry);

        _gameManager.Init(_winConditionTracker, _territoryManager, _matchState);
        _leaderBoardView.Init(_leaderBoardModel);
        _crownController.Init(_leaderBoardModel);

        if (_devTools != null)
        {
            _devTools.Init(_winConditionTracker, _enemySpawner, allCharacters, _matchState);
            _playerSpawner.CharacterSpawned += _devTools.OnPlayerSpawned;
        }
    }

    // Захват — доменное событие персонажа; волновая анимация — реакция презентации на него
    private void OnCharacterSpawned(ICharacter character)
    {
        character.AreaCaptured += _captureWavePresenter.OnAreaCaptured;
    }

    private void OnDestroy()
    {
        if (_playerSpawner != null)
            _playerSpawner.CharacterSpawned -= OnCharacterSpawned;

        if (_enemySpawner != null)
            _enemySpawner.CharacterSpawned -= OnCharacterSpawned;

        _leaderBoardModel?.Dispose();
        _winConditionTracker?.Dispose();
    }
}
