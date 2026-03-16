using UnityEngine;

public class PlayerSpawner : CharacterSpawner<Player>
{
    [SerializeField] private PlayerStatsView _uiPrefab;
    [SerializeField] private Transform _spawnPoint;

    private Transform _uiRoot;
    private PlayerStatsPresenter _presenter;

    public void SetUIRoot(Transform uiRoot)
    {
        _uiRoot = uiRoot;
    }

    private void Start()
    {
        EnsureInitialized();
        var hex = (_spawnPoint != null ? _grid.GetHexAt(_spawnPoint.position) : null) ?? _grid.GetRandomHex();
        SetSpawnHexes(new[] { hex });
        var player = SpawnSingleCharacter();
        player.SetName("Player");
        RegisterInLeaderBoard(player);
        InitUI(player);
    }

    private void InitUI(Player player)
    {
        var statsModel = player.StatsComponent.Stats;

        var view = Instantiate(_uiPrefab, _uiRoot);

        _presenter = new PlayerStatsPresenter(statsModel, view);
    }

    private void OnDestroy()
    {
        _presenter?.Dispose();
    }
}
