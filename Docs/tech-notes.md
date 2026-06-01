# 51% — Технические заметки

Детали реализации по эпикам. Не план — план в PLAN.md.

---

## Эпик 1 — Бустеры

### Иерархия классов

```
CollectibleBase
└── BoosterBase (абстрактный)
    ├── SpeedBooster
    ├── WingsBooster
    ├── SpikesBooster
    ├── MeteorBooster
    ├── EarthquakeBooster
    ├── TrailCutterBooster
    ├── CaptureRakeBooster
    └── MushroomBooster
```

### BoosterBase

```csharp
public abstract class BoosterBase : CollectibleBase
{
    [SerializeField] protected float _duration = 5f;
    public float Duration => _duration;

    public abstract void Apply(CharacterBase target, IBoosterContext context);
    public abstract void Revert(CharacterBase target);
}
```

### IBoosterContext

Некоторые бустеры (Meteor, Earthquake) требуют доступа к TerritoryManager и Grid.
Передаётся через интерфейс при инициализации в Bootstrap.

```csharp
public interface IBoosterContext
{
    TerritoryManager TerritoryManager { get; }
    IHexGridProvider Grid { get; }
    IKillManager KillManager { get; }
}
```

### BoosterHandler — компонент на персонаже

Управляет активным бустером: запускает таймер, вызывает Revert.
Добавить как `[RequireComponent]` в CharacterBase.

```csharp
public class BoosterHandler : MonoBehaviour
{
    private BoosterBase _activeBooster;
    private Coroutine _revertCoroutine;

    public bool HasActiveBooster => _activeBooster != null;

    public void Activate(BoosterBase booster, CharacterBase owner, IBoosterContext context)
    {
        if (_activeBooster != null)
            ForceRevert(owner);

        _activeBooster = booster;
        booster.Apply(owner, context);
        _revertCoroutine = StartCoroutine(RevertAfterDelay(booster, owner));
    }

    private IEnumerator RevertAfterDelay(BoosterBase booster, CharacterBase owner)
    {
        yield return new WaitForSeconds(booster.Duration);
        booster.Revert(owner);
        _activeBooster = null;
    }

    public void ForceRevert(CharacterBase owner)
    {
        if (_activeBooster == null) return;
        StopCoroutine(_revertCoroutine);
        _activeBooster.Revert(owner);
        _activeBooster = null;
    }
}
```

### CharacterBase — новые методы

```csharp
public void SetSpeedMultiplier(float multiplier) => _mover.SetSpeedMultiplier(multiplier);
public void SetTrailInvincible(bool value) => _conqueror.SetInvincible(value);
public void SetSpikesActive(bool value) => _conqueror.SetSpikesActive(value);
public void SetTrailWidth(int width) => _conqueror.SetTrailWidth(width);
```

Подключение в CharacterBase.OnItemCollected:

```csharp
case BoosterBase booster:
    _boosterHandler.Activate(booster, this, _boosterContext);
    break;
```

### SpeedBooster

Apply → `target.SetSpeedMultiplier(_multiplier)`
Revert → `target.SetSpeedMultiplier(1f)`
В Mover.cs добавить `_speedMultiplier`, умножать на базовую скорость в FixedUpdate.

### WingsBooster

Apply → установить флаг `_hasRevive = true` на персонаже.
KillManager вызывает `victim.Die()` → CharacterBase.Die() проверяет `_boosterHandler.TryRevive()`.
Если true → телепортировать на свободный гекс, вызвать `TerritoryManager.GiveStartTerritory(character, hex)`.
Revert → сбросить флаг (если не использован).

### SpikesBooster

Apply → `target.SetSpikesActive(true)`
В Conqueror: если `_spikesActive == true` и враг наступил на трейл → `KillManager.OnTrailInterrupted(victim, owner)`.
Revert → `target.SetSpikesActive(false)`

### MeteorBooster

Apply → активировать режим прицеливания.
Для бота: выбрать цель автоматически (скопление чужих гексов).
LaunchMeteor(Vector3) → spawn prefab, анимация DOTween, по приземлении:
`grid.GetHexesInRadius(targetCoord, _radius)` → `territoryManager.ReleaseHexes(hexes)`
Revert — нечего откатывать (одноразовый).

### EarthquakeBooster

Apply → у всех персонажей вызвать `BoosterHandler.ForceRevert(owner)`.
Revert — нечего откатывать.

### TrailCutterBooster

Apply → активировать режим прицеливания, выпустить снаряд.
При попадании в гекс с HexState.PartOfTrail:
`owner.Conqueror.CutTrailFrom(hitHex)` → удалить гексы трейла от hitHex до хвоста.
`territoryManager.ReleaseHexes(cutHexes)`
Новый метод в Conqueror: `CutTrailFrom(IHex hex)`.

### CaptureRakeBooster

Apply → `target.SetTrailWidth(3)`
В Conqueror при добавлении гекса в трейл: если `_rakeActive`, добавить 2 перпендикулярных соседа
(только если Empty или чужие, не домашняя территория).
Направление — из `Mover.CurrentDirection`.
Revert → `target.SetTrailWidth(1)`

### MushroomBooster

Apply → увеличить localScale персонажа + `target.SetTrailWidth(2)` (или другой множитель).
Revert → вернуть scale и ширину трейла.

### BoosterSpawner

По аналогии с CoinSpawner:
1. `_boosterPrefabs` — список всех бустеров
2. `SpawnBooster()` → случайный нейтральный гекс через `grid.GetRandomEmptyHex()`
3. Случайный тип из `_boosterPrefabs`
4. Spawn через ObjectPool
5. Лимит `_maxBoosters` на карте
6. После сбора → spawn с задержкой

### AI — бустеры

В BotContext добавить `bool HasActiveBooster`.
AttackEnemy: если у бота SpikesBooster → повышенный utility-скор атаки.
ReturnHome: если у бота WingsBooster → игнорировать опасность.

---

## Эпик 8 — Архитектурные улучшения

### State pattern для EnemyBrain

Текущая проблема: `BotState` enum + switch внутри `EnemyBrain.Think()`. Добавление нового состояния требует редактировать сам класс → нарушение OCP.

**Интерфейс:**

```csharp
public interface IEnemyState
{
    void Enter(EnemyBrain brain);
    void Execute(EnemyBrain brain);
    void Exit(EnemyBrain brain);
}
```

**EnemyBrain становится контекстом:**

```csharp
public class EnemyBrain : VectorProviderComponent
{
    private IEnemyState _currentState;

    public void TransitionTo(IEnemyState newState)
    {
        _currentState?.Exit(this);
        _currentState = newState;
        _currentState.Enter(this);
    }

    private void Think()
    {
        _currentState?.Execute(this);
    }
}
```

**Состояния:** `ExpandingState`, `ReturningState`, `AttackingState`, `CollectingState` — каждое сам инициирует переход через `brain.TransitionTo(new OtherState(...))`.

Без статики: состояния инстанциируются явно, `new ExpandingState()`. Не синглтоны, не static.

---

### CharacterFactory

Текущая проблема: `CharacterSpawner` конструирует, конфигурирует и регистрирует персонажа в одном месте. При добавлении respawn / dynamic difficulty логика дублируется.

**Фабрика отвечает только за сборку:**

```csharp
public class CharacterFactory
{
    private readonly ColorService _colorService;
    private readonly TerritoryManager _territoryManager;
    private readonly IHexGridProvider _grid;

    public CharacterFactory(ColorService colorService, TerritoryManager territoryManager, IHexGridProvider grid)
    {
        _colorService = colorService;
        _territoryManager = territoryManager;
        _grid = grid;
    }

    public TCharacter Create<TCharacter>(GameObject prefab, Vector3 position) where TCharacter : CharacterBase
    {
        var go = Object.Instantiate(prefab, position, Quaternion.identity);
        var character = go.GetComponent<TCharacter>();
        character.Init(_colorService, _territoryManager, _grid);
        return character;
    }
}
```

**Spawner остаётся координатором:**

```csharp
// CharacterSpawner
var character = _factory.Create<Enemy>(_prefab, spawnHex.WorldPosition);
_territoryManager.GetStartTerritory(character, spawnHex);
_killManager.Register(character);
_winConditionTracker.Register(character);
```

Bootstrap создаёт `CharacterFactory` явно и передаёт в оба спавнера.

---

### Bootstrap → Module Installers

Текущая проблема: Bootstrap растёт при добавлении новых систем, превращаясь в God Object.

**Каждый Installer собирает свою подсистему:**

```csharp
public class GridInstaller
{
    public HexGrid Install(HexGrid hexGridRef, HexNeighborOffsets offsets)
    {
        // конфигурация HexGrid
        return hexGridRef;
    }
}

public class CharacterInstaller
{
    public (PlayerSpawner, EnemySpawner) Install(
        CharacterFactory factory, TerritoryManager territory, KillManager kill, ...)
    {
        var playerSpawner = ...;
        var enemySpawner = ...;
        return (playerSpawner, enemySpawner);
    }
}

public class AIInstaller
{
    public void Install(EnemySpawner spawner, ICollectibleRegistry registry, ...) { ... }
}
```

**Bootstrap — только оркестратор порядка:**

```csharp
private void Awake()
{
    var grid = new GridInstaller().Install(_hexGrid, _neighborOffsets);
    var territory = new TerritoryInstaller().Install(grid);
    var (playerSpawner, enemySpawner) = new CharacterInstaller().Install(_factory, territory, ...);
    new AIInstaller().Install(enemySpawner, _collectibleRegistry, ...);
}
```

Installers — обычные классы (не MonoBehaviour, не static), инстанциируются в Bootstrap.Awake().

---

## Эпик 2 — UI игровой сцены

### Система очков

```csharp
public static int GetPlaceScore(int place) => place switch
{
    1 => 600, 2 => 500, 3 => 400,
    4 => 300, 5 => 200, _ => 100
};
```

Место определяется в WinConditionTracker / LeaderBoardModel на момент завершения.
Передаётся в `EndgameWindow.Show(int place, int score)`.

### EndgameWindow

- Анимация: DOTween fade-in + scale punch на заголовке
- Показывать место и очки
- Кнопка "Забрать награду" / "Умножить на 2" (rewarded ad)
- Application.Quit() → заменить на переход в MainMenu после подключения SDK

### PauseWindow

- Добавить кнопку паузы в HUD (для WebGL — Escape недоступен)
- SettingsClicked → открывать SettingsWindow
- Анимация появления

### BoostViewer

- Подписывается на BoosterHandler.BoosterActivated / BoosterReverted
- Иконка бустера (поле `_icon` в BoosterBase)
- Таймбар: `DOFillAmount(0f, booster.Duration)`
- Скрывается когда бустер неактивен

---

## Эпик 3 — Главное меню

### Переход между сценами

Bootstrap в Game сцене. MainMenu — отдельная лёгкая сцена.
Данные между сценами: PlayerPrefs (имя) + статичный PlayerData (выбранный скин).

```csharp
public static class PlayerData
{
    public static string Name { get; set; }
    public static int SelectedSkinIndex { get; set; }
}
```

### Экран загрузки

`SceneLoader.cs` — async загрузка через `SceneManager.LoadSceneAsync`.
Прогресс-бар или анимация пока сцена грузится.

---

## Эпик 7 — Платформа

### IPlatformService

```csharp
public interface IPlatformService
{
    string GetLanguage();
    void ReportScore(int score);
    void ShowAd(Action onComplete);
    void GameReady();
}

public class YandexPlatformService : IPlatformService { ... }
public class EditorPlatformService : IPlatformService { ... }
```

Bootstrap создаёт нужную реализацию по `Application.platform`.

### Локализация

```csharp
public static class Localization
{
    public enum Lang { RU, EN, TR }
    public static Lang Current { get; private set; } = Lang.RU;

    private static readonly Dictionary<string, string[]> Keys = new()
    {
        ["victory"]    = new[] { "Победа!",     "Victory!",  "Zafer!"     },
        ["defeat"]     = new[] { "Поражение!",  "Defeat!",   "Yenilgi!"   },
        ["restart"]    = new[] { "Заново",      "Restart",   "Yeniden"    },
        ["continue"]   = new[] { "Продолжить",  "Continue",  "Devam Et"   },
        ["exit"]       = new[] { "Выйти",       "Exit",      "Çıkış"      },
        ["pause"]      = new[] { "Пауза",       "Pause",     "Duraklat"   },
        ["territory"]  = new[] { "Территория",  "Territory", "Bölge"      },
        ["kills"]      = new[] { "Убийства",    "Kills",     "Öldürmeler" },
        ["coins"]      = new[] { "Монеты",      "Coins",     "Paralar"    },
        ["play"]       = new[] { "Играть",      "Play",      "Oyna"       },
        ["enter_name"] = new[] { "Введи имя",   "Enter name","İsim girin" },
    };

    public static string Get(string key) =>
        Keys.TryGetValue(key, out var arr) ? arr[(int)Current] : key;

    public static void Set(string yandexLang) => Current = yandexLang switch
    {
        "en" => Lang.EN,
        "tr" => Lang.TR,
        _    => Lang.RU
    };
}
```

### Яндекс SDK — обязательные вызовы

| Функция | Метод PluginYG | Где |
|---------|---------------|-----|
| Уведомить о готовности | `YandexGame.GameReadyAPI()` | Bootstrap.Start() |
| Получить язык | `YandexGame.lang` | Bootstrap.Awake() → Localization.Set |
| Сохранить очки | `YandexGame.NewLeaderboardScores("main", score)` | После EndgameWindow.Show |
| Реклама перед рестартом | `YandexGame.FullscreenShow()` | GameManager.Restart() |
