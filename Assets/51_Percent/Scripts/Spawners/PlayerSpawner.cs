using UnityEngine;

public class PlayerSpawner : CharacterSpawner<Player>
{
    private const string PlayerName = "Player";

    [Required] [SerializeField] private PlayerStatsView _uiPrefab;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private PlayerBoosterHUD _boosterHud;
    [SerializeField] private BoosterIconRegistry _boosterIconRegistry;
    [SerializeField] private CameraFollower _cameraFollower;

    private Transform _uiRoot;
    private PlayerStatsView _statsView;
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
        SpawnPlayer();
    }

    // Воскрешение = полноценный новый персонаж; точка спавна исчерпана первым спавном,
    // поэтому новый падает на случайный гекс
    public Player Respawn()
    {
        return SpawnPlayer();
    }

    private Player SpawnPlayer()
    {
        var player = SpawnNext();
        player.SetName(PlayerName);
        RegisterInLeaderBoard(player);
        BindUI(player);
        _cameraFollower?.Init(player.Transform);
        return player;
    }

    // Вьюшка одна на всю сессию — при респавне презентеры перевязываются на нового персонажа
    private void BindUI(Player player)
    {
        if (_statsView == null)
            _statsView = Instantiate(_uiPrefab, _uiRoot);

        _statsPresenter?.Dispose();
        _statsPresenter = new PlayerStatsPresenter(player.LifeStats, _statsView);
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
