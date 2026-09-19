using System.Collections.Generic;
using System.Text;
using UnityEngine;

// Читы dev-сцены. Клавиши: 1-4 — выдать бустер из массива, 5 — заспавнить врага (он всегда один),
// 6 — натравить врага на игрока / остановить, 7 — победить, 8 — проиграть (нужен живой противник),
// 9 — выбросить бустер из кармана, 0 — снять активный эффект
public class DevTools : MonoBehaviour
{
    private const string LogPrefix = "[DevTools]";

    private readonly KeyCode[] _boosterKeys =
        { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4 };

    private const KeyCode SpawnEnemyKey = KeyCode.Alpha5;
    private const KeyCode ChaseKey = KeyCode.Alpha6;
    private const KeyCode WinKey = KeyCode.Alpha7;
    private const KeyCode LoseKey = KeyCode.Alpha8;
    private const KeyCode DropPendingKey = KeyCode.Alpha9;
    private const KeyCode DeactivateKey = KeyCode.Alpha0;

    [Header("Бустеры по клавишам 1-4 (в порядке массива)")]
    [SerializeField] private Booster[] _boosterPrefabs;

    private WinConditionTracker _winConditionTracker;
    private EnemySpawner _enemySpawner;
    private IReadOnlyList<ICharacter> _allCharacters;
    private ICharacter _player;
    private ICollectibleConsumer _playerConsumer;
    private BoosterHandler _playerBoosterHandler;
    private ICharacter _devEnemy;
    private DevChaseProvider _devChaseProvider;
    private string _helpText;
    private GUIStyle _helpStyle;
    private IMatchState _matchState;

    private void Awake()
    {
        _helpText = BuildHelpText();
    }

    public void Init(WinConditionTracker winConditionTracker,
        EnemySpawner enemySpawner, IReadOnlyList<ICharacter> allCharacters, IMatchState matchState)
    {
        _winConditionTracker = winConditionTracker;
        _enemySpawner = enemySpawner;
        _allCharacters = allCharacters;
        _matchState = matchState;
    }

    public void OnPlayerSpawned(ICharacter player)
    {
        _player = player;
        player.Transform.TryGetComponent(out _playerConsumer);
        player.Transform.TryGetComponent(out _playerBoosterHandler);
    }

    private void Update()
    {
        if (_matchState == null || !_matchState.IsRunning)
            return;

        for (int i = 0; i < _boosterKeys.Length; i++)
            if (Input.GetKeyDown(_boosterKeys[i]))
                GiveBooster(i);

        if (Input.GetKeyDown(SpawnEnemyKey))
            SpawnIdleEnemy();

        if (Input.GetKeyDown(ChaseKey))
            ToggleChase();

        if (Input.GetKeyDown(WinKey))
            FinishAsWin();

        if (Input.GetKeyDown(LoseKey))
            FinishAsLose();

        if (Input.GetKeyDown(DropPendingKey))
            DropPendingBooster();

        if (Input.GetKeyDown(DeactivateKey))
            DeactivateBooster();
    }

    private void GiveBooster(int index)
    {
        if (!IsPlayerAlive())
            return;

        if (_boosterPrefabs == null || index >= _boosterPrefabs.Length || _boosterPrefabs[index] == null)
        {
            Debug.Log($"{LogPrefix} Бустер под клавишей {index + 1} не назначен");
            return;
        }

        var booster = _boosterPrefabs[index];

        if (_playerConsumer.TryAcceptBooster(booster.CreateEffect()))
            Debug.Log($"{LogPrefix} Выдан бустер: {booster.name}");
        else
            Debug.Log($"{LogPrefix} Руки заняты — сначала потрать текущий бустер");
    }

    private void SpawnIdleEnemy()
    {
        if (_devEnemy != null && _devEnemy.State == CharacterState.Alive)
        {
            Debug.Log($"{LogPrefix} Враг уже на поле — он всегда один");
            return;
        }

        _devEnemy = _enemySpawner.SpawnIdleEnemy();
        _devChaseProvider = _devEnemy.Transform.gameObject.AddComponent<DevChaseProvider>();

        if (_devEnemy.Transform.TryGetComponent(out Mover mover))
            mover.SetProvider(_devChaseProvider);

        Debug.Log($"{LogPrefix} Заспавнен враг: {_devEnemy.Name}. Клавиша 6 — натравить на игрока");
    }

    private void ToggleChase()
    {
        if (_devEnemy == null || _devEnemy.State != CharacterState.Alive)
        {
            Debug.Log($"{LogPrefix} Нет противника — клавиша 5, чтобы заспавнить");
            return;
        }

        if (_devChaseProvider.HasTarget)
        {
            _devChaseProvider.ClearTarget();
            Debug.Log($"{LogPrefix} Враг остановлен");
            return;
        }

        if (!IsPlayerAlive())
            return;

        _devChaseProvider.SetTarget(_player.Transform);
        Debug.Log($"{LogPrefix} Враг бежит к игроку — подставь трейл, чтобы умереть");
    }

    private void FinishAsWin()
    {
        if (!IsPlayerAlive())
            return;

        _winConditionTracker.ForceFinish(_player);
    }

    private void FinishAsLose()
    {
        var winner = FindAliveEnemy();

        if (winner == null)
        {
            Debug.Log($"{LogPrefix} Поражение невозможно: нет противника, некому побеждать");
            return;
        }

        _winConditionTracker.ForceFinish(winner);
    }

    private void DropPendingBooster()
    {
        if (!IsPlayerAlive())
            return;

        if (!_playerBoosterHandler.HasPendingBooster)
        {
            Debug.Log($"{LogPrefix} В кармане пусто — выбрасывать нечего");
            return;
        }

        if (_playerBoosterHandler.TryDropPending())
            Debug.Log($"{LogPrefix} Бустер выброшен из кармана");
    }

    private void DeactivateBooster()
    {
        if (!IsPlayerAlive())
            return;

        if (!_playerBoosterHandler.HasActiveBooster)
        {
            Debug.Log($"{LogPrefix} Активного эффекта нет — снимать нечего");
            return;
        }

        if (_playerBoosterHandler.TryDeactivate())
            Debug.Log($"{LogPrefix} Активный эффект снят");
    }

    private string BuildHelpText()
    {
        var help = new StringBuilder();
        help.AppendLine("=== DevTools ===");

        for (int i = 0; i < _boosterKeys.Length; i++)
        {
            string boosterName = _boosterPrefabs != null && i < _boosterPrefabs.Length && _boosterPrefabs[i] != null
                ? _boosterPrefabs[i].name
                : "(не назначен)";
            help.AppendLine($"{i + 1} — бустер: {boosterName}");
        }

        help.AppendLine("5 — заспавнить врага (всегда один)");
        help.AppendLine("6 — натравить / остановить врага");
        help.AppendLine("7 — победа");
        help.AppendLine("8 — поражение");
        help.AppendLine("9 — выбросить бустер из кармана");
        help.Append("0 — снять активный эффект");

        return help.ToString();
    }

    private void OnGUI()
    {
        // GUIStyle нельзя создавать вне OnGUI — кэшируем при первом вызове
        _helpStyle ??= new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperLeft };

        var content = new GUIContent(_helpText);
        Vector2 size = _helpStyle.CalcSize(content);
        GUI.Box(new Rect(10f, Screen.height - size.y - 10f, size.x, size.y), content, _helpStyle);
    }

    private bool IsPlayerAlive()
    {
        if (_player != null && _player.State == CharacterState.Alive)
            return true;

        Debug.Log($"{LogPrefix} Игрок не заспавнен или мёртв");
        return false;
    }

    private ICharacter FindAliveEnemy()
    {
        foreach (var character in _allCharacters)
            if (character != null && !character.IsHuman && character.State == CharacterState.Alive)
                return character;

        return null;
    }
}
