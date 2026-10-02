using UnityEngine;

// Владелец презентации игрока: интерфейс живёт всю сессию, а персонаж за это время
// сменяется при респавне, поэтому вьюшка создаётся один раз, а презентеры перевязываются
// на нового персонажа. Спавнер об этом не знает — он только сообщает о появлении.
public class PlayerHudBinder : MonoBehaviour
{
    [Required] [SerializeField] private PlayerStatsView _statsViewPrefab;
    [Required] [SerializeField] private Transform _uiRoot;
    [SerializeField] private PlayerBoosterHUD _boosterHud;
    [SerializeField] private BoosterIconRegistry _boosterIconRegistry;
    [SerializeField] private CameraFollower _cameraFollower;

    private PlayerSpawner _playerSpawner;
    private PlayerStatsView _statsView;
    private PlayerStatsPresenter _statsPresenter;
    private PlayerBoosterHudPresenter _boosterPresenter;

    public void Init(PlayerSpawner playerSpawner)
    {
        _playerSpawner = playerSpawner != null
            ? playerSpawner
            : throw new System.ArgumentNullException(nameof(playerSpawner));

        _playerSpawner.CharacterSpawned += OnPlayerSpawned;
    }

    private void OnPlayerSpawned(ICharacter player)
    {
        BindStats(player);
        BindBoosterHud(player);
        _cameraFollower?.Init(player.Transform);
    }

    // Вьюшка одна на всю сессию: пересоздание сбросило бы её место в иерархии UI
    private void BindStats(ICharacter player)
    {
        if (_statsView == null)
            _statsView = Instantiate(_statsViewPrefab, _uiRoot);

        _statsPresenter?.Dispose();
        _statsPresenter = new PlayerStatsPresenter(player.LifeStats, _statsView);
    }

    private void BindBoosterHud(ICharacter player)
    {
        if (_boosterHud == null || _boosterIconRegistry == null)
            return;

        _boosterPresenter?.Dispose();
        _boosterPresenter = new PlayerBoosterHudPresenter(player.BoosterObservable, _boosterHud, _boosterIconRegistry);
        _boosterHud.Bind(_boosterPresenter);
    }

    private void OnDestroy()
    {
        if (_playerSpawner != null)
            _playerSpawner.CharacterSpawned -= OnPlayerSpawned;

        _statsPresenter?.Dispose();
        _boosterPresenter?.Dispose();
    }
}
