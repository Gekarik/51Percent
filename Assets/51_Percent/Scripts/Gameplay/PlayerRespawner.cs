using System;
using DG.Tweening;
using UnityEngine;

// Оркестратор воскрешения: смерть игрока → пауза → новый персонаж (полноценный,
// через фабрику спавнера). Пока у нового персонажа открыто окно приземления, он не
// участвует в игре, а его стартовую территорию охраняет кольцо стен. Срок окна
// принадлежит самому персонажу — здесь он не дублируется
public class PlayerRespawner : MonoBehaviour, IRespawnScheduler
{
    [SerializeField] private int _respawnCount = 1;
    [SerializeField] private float _delayBeforeSpawn = 2f;
    [SerializeField] private float _shieldRadius = 4f;
    [SerializeField] private float _shieldHeight = 3f;

    private readonly RespawnShieldBuilder _shieldBuilder = new RespawnShieldBuilder();

    private PlayerSpawner _playerSpawner;
    private Player _landingPlayer;
    private int _respawnsLeft;
    private bool _hasPendingRespawn;
    private GameObject _shield;
    private IMatchState _matchState;
    private Tween _pendingSpawn;

    public bool HasPendingRespawn => _hasPendingRespawn;

    public void Init(PlayerSpawner playerSpawner, IMatchState matchState)
    {
        _playerSpawner = playerSpawner ?? throw new ArgumentNullException(nameof(playerSpawner));
        _matchState = matchState ?? throw new ArgumentNullException(nameof(matchState));
        _matchState.Changed += OnMatchChanged;
        _respawnsLeft = _respawnCount;
    }

    // Координатор выбывания вызывает это до проверки победы.
    public bool TryScheduleRespawn(ICharacter character)
    {
        if (!character.IsHuman || character.State != CharacterState.Died || _respawnsLeft <= 0
            || _hasPendingRespawn || _matchState.IsFinished)
            return false;

        _respawnsLeft--;
        _hasPendingRespawn = true;
        _pendingSpawn = DOVirtual.DelayedCall(_delayBeforeSpawn, SpawnNewPlayer, ignoreTimeScale: false);
        return true;
    }

    private void SpawnNewPlayer()
    {
        _pendingSpawn = null;
        if (_matchState.IsFinished)
        {
            _hasPendingRespawn = false;
            return;
        }

        _landingPlayer = _playerSpawner.Respawn();
        _hasPendingRespawn = false;

        _shield = _shieldBuilder.Build(_landingPlayer.Transform.position, _shieldRadius, _shieldHeight);
        _landingPlayer.LandingFinished += RemoveShield;
    }

    private void RemoveShield()
    {
        if (_landingPlayer != null)
            _landingPlayer.LandingFinished -= RemoveShield;
        _landingPlayer = null;

        if (_shield == null)
            return;

        Destroy(_shield);
        _shield = null;
    }

    private void OnMatchChanged()
    {
        if (!_matchState.IsFinished)
            return;

        _pendingSpawn?.Kill();
        _pendingSpawn = null;
        _hasPendingRespawn = false;
    }

    private void OnDestroy()
    {
        if (_matchState != null)
            _matchState.Changed -= OnMatchChanged;

        _pendingSpawn?.Kill();
        RemoveShield();
    }
}
