using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Адаптивный контроллер бота с системой состояний
/// Имитирует поведение реального игрока в io-игре
/// </summary>
[RequireComponent(typeof(Mover), typeof(Conquester), typeof(AdaptivePathProvider))]
public class AdaptiveBotController : MonoBehaviour
{
    [SerializeField] private BotStateConfig _config = new BotStateConfig();
    [SerializeField] private bool _enableDebugLogs = false;
    
    private Conquester _conquester;
    private AdaptivePathProvider _pathProvider;
    private SituationAnalyzer _analyzer;
    private IHexGridProvider _grid;
    private ICharacter _character;
    private System.Random _random;
    
    private BotState _currentState = BotState.Idle;
    private float _stateTimer;
    private float _decisionTimer;
    private const float DECISION_INTERVAL = 0.8f; // Более частые решения для плавности
    
    public BotState CurrentState => _currentState;
    public BotStateConfig Config => _config;
    
    private void Awake()
    {
        _conquester = GetComponent<Conquester>();
        _pathProvider = GetComponent<AdaptivePathProvider>();
        _character = GetComponent<ICharacter>();
        
        // Инициализируем рандом с уникальным семенем для каждого бота
        _random = new System.Random(GetInstanceID());
    }
    
    public void Init(IHexGridProvider gridProvider)
    {
        _grid = gridProvider;
        _analyzer = new SituationAnalyzer(_grid, _config);
        StartCoroutine(BotLoop());
    }
    
    /// <summary>
    /// Основной цикл принятия решений бота (оптимизированный)
    /// </summary>
    private IEnumerator BotLoop()
    {
        // Добавляем случайную задержку для распределения нагрузки между ботами
        yield return new WaitForSeconds(_random.Next(0, 100) / 100f);
        
        while (_character.State == CharacterState.Alive)
        {
            _decisionTimer += Time.deltaTime;
            _stateTimer += Time.deltaTime;
            
            // Принимаем решение о смене состояния
            if (_decisionTimer >= DECISION_INTERVAL)
            {
                _decisionTimer = 0f;
                DecideNextState();
            }
            
            // Выполняем действия текущего состояния
            ExecuteCurrentState();
            
            // Пропускаем несколько кадров для распределения нагрузки
            yield return new WaitForSeconds(0.2f);
        }
    }
    
    /// <summary>
    /// Принимает решение о следующем состоянии на основе ситуации
    /// </summary>
    private void DecideNextState()
    {
        Vector3 position = transform.position;
        
        // Debug информация
        if (_enableDebugLogs) Debug.Log($"Bot {gameObject.name} deciding state. Current: {_currentState}, Timer: {_stateTimer:F1}");
        
        // 1. Проверяем критические ситуации (побег)
        if (_analyzer.IsThreatenedByEnemy(_character, position, _config.detectionRange))
        {
            if (_random.Next(0, 100) < _config.escapeThreshold)
            {
                Debug.Log($"Bot {gameObject.name} escaping from threat!");
                ChangeState(BotState.Escape);
                return;
            }
        }
        
        // 2. Оцениваем приоритеты всех состояний
        var statePriorities = new Dictionary<BotState, int>();
        
        foreach (BotState state in System.Enum.GetValues(typeof(BotState)))
        {
            if (state == BotState.Idle) continue;
            
            int priority = _analyzer.EvaluateStatePriority(state, _character, position);
            int threshold = _config.GetStateThreshold(state);
            
            // Применяем рандомность на основе агрессивности
            if (_config.ShouldPerformAction(state, _random))
            {
                priority += _random.Next(-10, 20); // Добавляем случайность
            }
            else
            {
                priority = 0; // Бот "не хочет" выполнять это действие
            }
            
            statePriorities[state] = priority;
            if (_enableDebugLogs) Debug.Log($"State {state} priority: {priority}");
        }
        
        // 3. Выбираем состояние с наивысшим приоритетом
        BotState bestState = BotState.Idle;
        int maxPriority = 0;
        
        foreach (var kvp in statePriorities)
        {
            if (kvp.Value > maxPriority)
            {
                maxPriority = kvp.Value;
                bestState = kvp.Key;
            }
        }
        
        if (_enableDebugLogs) Debug.Log($"Best state: {bestState} with priority {maxPriority}");
        
        // 4. Меняем состояние, если приоритет достаточно высок или долго в idle
        if (maxPriority > 30 || _stateTimer > _config.idleTime * 2)
        {
            ChangeState(bestState);
        }
        else if (maxPriority <= 30)
        {
            // Принудительно активируем Expand если нет других приоритетов
            if (_enableDebugLogs) Debug.Log($"Forcing Expand state due to low priorities");
            ChangeState(BotState.Expand);
        }
    }
    
    /// <summary>
    /// Меняет текущее состояние бота
    /// </summary>
    private void ChangeState(BotState newState)
    {
        if (_currentState == newState) return;
        
        // Выходим из предыдущего состояния
        ExitState(_currentState);
        
        // Входим в новое состояние
        _currentState = newState;
        _stateTimer = 0f;
        EnterState(_currentState);
    }
    
    /// <summary>
    /// Выполняет действия при входе в состояние
    /// </summary>
    private void EnterState(BotState state)
    {
        Vector3 position = transform.position;
        
        switch (state)
        {
            case BotState.Collect:
                var resources = _analyzer.FindNearbyResources(position, _config.detectionRange);
                if (resources.Count > 0)
                {
                    _pathProvider.SetResourceTarget(resources[0]);
                }
                break;
                
            case BotState.Attack:
                var trails = _analyzer.FindVulnerableEnemyTrails(_character, position, _config.detectionRange);
                if (trails.Count > 0)
                {
                    _pathProvider.SetHexTarget(trails[0]);
                }
                break;
                
            case BotState.Expand:
                var expansionTarget = FindExpansionTarget();
                if (expansionTarget != null)
                {
                    _pathProvider.SetHexTarget(expansionTarget);
                    Debug.Log($"Bot {gameObject.name} expanding towards hex at logical distance: {_grid.Distance(GetNearestOwnedHex(), expansionTarget as Hex)} steps");
                }
                break;
                
            case BotState.Escape:
                var escapePath = CreateEscapePath();
                if (escapePath != null)
                {
                    _pathProvider.SetPath(escapePath);
                }
                break;
                
            case BotState.Return:
                var returnPath = CreateReturnPath();
                if (returnPath != null)
                {
                    _pathProvider.SetPath(returnPath);
                }
                break;
        }
    }
    
    /// <summary>
    /// Выполняет действия при выходе из состояния
    /// </summary>
    private void ExitState(BotState state)
    {
        // Можно добавить очистку ресурсов или логирование
    }
    
    /// <summary>
    /// Выполняет действия текущего состояния
    /// </summary>
    private void ExecuteCurrentState()
    {
        switch (_currentState)
        {
            case BotState.Idle:
                if (_stateTimer > _config.idleTime)
                {
                    ChangeState(BotState.Expand); // По умолчанию переходим к расширению
                }
                break;
                
            case BotState.Collect:
            case BotState.Attack:
            case BotState.Expand:
            case BotState.Escape:
            case BotState.Return:
                // Проверяем, завершено ли движение
                if (_pathProvider.IsDone)
                {
                    ChangeState(BotState.Idle);
                }
                break;
        }
    }
    
    /// <summary>
    /// Находит цель для расширения в заданном направлении (дальняя цель)
    /// </summary>
    private IHex FindExpansionTarget()
    {
        var ownedHexes = _conquester.FixedHexes;
        Debug.Log($"Bot {gameObject.name} FindExpansionTarget: owned hexes count = {ownedHexes.Count}");
        
        if (ownedHexes.Count == 0) 
        {
            Debug.LogWarning($"Bot {gameObject.name} has no owned hexes! Cannot expand.");
            return null;
        }
        
        // Выбираем случайный край территории как стартовую точку
        var borderHexes = GetBorderHexes();
        if (borderHexes.Count == 0)
        {
            Debug.LogWarning($"Bot {gameObject.name} no border hexes found!");
            return null;
        }
        
        var startHex = borderHexes[_random.Next(borderHexes.Count)];
        
        // Ищем цель НА ГРАНИЦЕ maxTrailLength, а не ближайшую
        IHex bestTarget = null;
        int targetDistance = _config.maxTrailLength; // Целевая дистанция
        int bestScore = -1;
        int emptyHexesFound = 0;
        
        Debug.Log($"Bot {gameObject.name} searching for target at ~{targetDistance} steps from border");
        
        foreach (var hex in _grid.AllHexes)
        {
            if (hex.State != HexState.Empty && hex.Owner == _character) continue; // Пропускаем свои гексы
            emptyHexesFound++;
            
            int logicalDistance = _grid.Distance(startHex, hex as Hex);
            
            // Предпочитаем цели близко к maxTrailLength (дальние цели)
            int distanceScore = 100 - Mathf.Abs(logicalDistance - targetDistance);
            
            // Бонус за пустые гексы
            int stateScore = 0;
            if (hex.State == HexState.Empty) stateScore = 50;
            else if (hex.Owner != _character) stateScore = 30; // Вражеские гексы тоже можно захватывать
            
            int totalScore = distanceScore + stateScore;
            
            if (totalScore > bestScore && logicalDistance <= _config.maxTrailLength)
            {
                bestScore = totalScore;
                bestTarget = hex;
            }
        }
        
        int finalDistance = bestTarget != null ? _grid.Distance(startHex, bestTarget as Hex) : -1;
        Debug.Log($"Bot {gameObject.name} found target at {finalDistance} steps (score: {bestScore}, empty hexes: {emptyHexesFound})");
        
        return bestTarget;
    }
    
    /// <summary>
    /// Получает ближайший принадлежащий боту гекс
    /// </summary>
    private IHex GetNearestOwnedHex()
    {
        var ownedHexes = _conquester.FixedHexes;
        if (ownedHexes.Count == 0) return null;
        
        return ownedHexes.OrderBy(h => Vector3.Distance(transform.position, h.transform.position)).First() as IHex;
    }
    
    /// <summary>
    /// Получает гексы на границе территории
    /// </summary>
    private List<IHex> GetBorderHexes()
    {
        var ownedHexes = _conquester.FixedHexes.Cast<IHex>().ToList();
        var borderHexes = new List<IHex>();
        
        foreach (var hex in ownedHexes)
        {
            var neighbors = _grid.GetNeighbors(hex);
            bool isBorder = neighbors.Any(n => n.Owner != _character);
            
            if (isBorder)
            {
                borderHexes.Add(hex);
            }
        }
        
        return borderHexes;
    }
    
    /// <summary>
    /// Создает путь для побега
    /// </summary>
    private List<IHex> CreateEscapePath()
    {
        var ownedHexes = _conquester.FixedHexes;
        if (ownedHexes.Count == 0) return null;
        
        // Найти ближайшую безопасную территорию
        var closestSafe = ownedHexes.OrderBy(h => Vector3.Distance(transform.position, h.transform.position)).First() as IHex;
        var currentHex = _grid.AllHexes.OrderBy(h => Vector3.Distance(transform.position, h.transform.position)).First();
        
        return Pathfinder.AStar(currentHex, closestSafe, _grid, h => h.State != HexState.PartOfTrail || h.Owner == _character);
    }
    
    /// <summary>
    /// Создает путь для возврата на территорию
    /// </summary>
    private List<IHex> CreateReturnPath()
    {
        // Упрощенная версия - возвращаемся к ближайшей точке территории
        return CreateEscapePath();
    }

}