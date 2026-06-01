using UnityEngine;

public class PlayerSpawner : CharacterSpawner<Player>
{
    [SerializeField] private PlayerStatsView _uiPrefab;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private PlayerBoosterHUD _boosterHud;
    [SerializeField] private BoosterIconRegistry _boosterIconRegistry;
    [SerializeField] private CameraFollower _cameraFollower;

    private Transform _uiRoot;
    private PlayerStatsPresenter _statsPresenter;
    private PlayerBoosterHudPresenter _boosterPresenter;

    public void SetUIRoot(Transform uiRoot)
    {
        _uiRoot = uiRoot;
    }

    private void Start()
    {
        EnsureInitialized();
        var hex = (_spawnPoint != null ? _grid.GetHexAt(_spawnPoint.position) : null) ?? _grid.GetRandomHex();
        SetSpawnHexes(new[] { hex });

        var player = SpawnNext();
        player.SetName("Player");
        RegisterInLeaderBoard(player);
        InitUI(player);
        _cameraFollower?.Init(player.Transform);
    }

    protected override void OnRespawnRequested(Player character)
    {
        var newPlayer = SpawnAt(_grid.GetRandomHex());
        newPlayer.SetName(character.Name);
        RegisterInLeaderBoard(newPlayer);
        BindBoosterHud(newPlayer);
        _cameraFollower?.Init(newPlayer.Transform);
    }

    private void InitUI(Player player)
    {
        var view = Instantiate(_uiPrefab, _uiRoot);
        _statsPresenter = new PlayerStatsPresenter(player.StatsComponent.Stats, view);
        BindBoosterHud(player);
    }

    private void BindBoosterHud(Player player)
    {
        if (_boosterHud == null || _boosterIconRegistry == null)
            return;

        _boosterPresenter?.Dispose();
        _boosterPresenter = new PlayerBoosterHudPresenter(player.BoosterObservable, _boosterHud, _boosterIconRegistry);
        _boosterHud.Bind(_boosterPresenter);
    }

    private void OnDestroy()
    {
        _statsPresenter?.Dispose();
        _boosterPresenter?.Dispose();
    }
}
