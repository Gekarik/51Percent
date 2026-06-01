using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class Bootstrap : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private HexGrid _hexGrid;
    [SerializeField] private TerritoryManager _territoryManager;
    [SerializeField] private PlayerSpawner _playerSpawner;
    [SerializeField] private EnemySpawner _enemySpawner;
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private CoinSpawner _coinSpawner;
    [SerializeField] private BoosterSpawner _boosterSpawner;

    [Header("Settings")]
    [SerializeField] private ColorSettings _colorSettings;

    [Header("UI")]
    [SerializeField] private Transform _uiRoot;
    [SerializeField] private LeaderBoardView _leaderBoardView;
    [SerializeField] private CrownController _crownController;

    private ColorService _colorService;
    private KillManager _killManager;
    private LeaderBoardModel _leaderBoardModel;
    private WinConditionTracker _winConditionTracker;

    private void Awake()
    {
        _colorService = new ColorService(_colorSettings);
        _leaderBoardModel = new LeaderBoardModel(_territoryManager);
        _winConditionTracker = new WinConditionTracker(_territoryManager);
        _killManager = new KillManager();

        var allCharacters = new List<ICharacter>();
        var collectibleRegistry = new CollectibleRegistry();

        _killManager.CharacterEliminated += _leaderBoardModel.UnregisterCharacter;
        _killManager.CharacterEliminated += _winConditionTracker.OnCharacterEliminated;
        _killManager.CharacterEliminated += OnCharacterEliminated;

        _playerSpawner.SetCharacterList(allCharacters);
        _playerSpawner.Init(_hexGrid, _territoryManager, _killManager, _colorService, _leaderBoardModel, _winConditionTracker);
        _playerSpawner.SetUIRoot(_uiRoot);

        _enemySpawner.SetCharacterList(allCharacters);
        _enemySpawner.SetAIReferences(allCharacters, collectibleRegistry);
        _enemySpawner.Init(_hexGrid, _territoryManager, _killManager, _colorService, _leaderBoardModel, _winConditionTracker);

        _coinSpawner?.SetRegistry(collectibleRegistry);
        _boosterSpawner?.SetRegistry(collectibleRegistry);

        _gameManager.Init(_winConditionTracker, _territoryManager);
        _leaderBoardView.Init(_leaderBoardModel);
        _crownController.Init(_leaderBoardModel);
    }

    private void OnCharacterEliminated(ICharacter character)
    {
        int coins = character.StatsComponent.Stats.Coins;
        if (coins > 0 && _coinSpawner != null)
            _coinSpawner.ScatterCoins(character.Transform.position, coins);
    }

    private void OnDestroy()
    {
        if (_killManager != null)
        {
            _killManager.CharacterEliminated -= _leaderBoardModel.UnregisterCharacter;
            _killManager.CharacterEliminated -= _winConditionTracker.OnCharacterEliminated;
            _killManager.CharacterEliminated -= OnCharacterEliminated;
        }

        _leaderBoardModel?.Dispose();
        _winConditionTracker?.Dispose();
    }
}
