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
    private const float DECISION_INTERVAL = 1.5f; // Уменьшена частота до 1.5 секунд для производительности
    
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
            yield return new WaitForSeconds(0.1f);
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
                var expansionPath = CreateExpansionPath();
                if (expansionPath != null)
                {
                    _pathProvider.SetPath(expansionPath);
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
    /// Создает простой путь для расширения территории (оптимизированный)
    /// </summary>
    private List<IHex> CreateExpansionPath()
    {
        var ownedHexes = _conquester.FixedHexes;
        Debug.Log($"Bot {gameObject.name} CreateExpansionPath: owned hexes count = {ownedHexes.Count}");
        
        if (ownedHexes.Count == 0) 
        {
            Debug.LogWarning($"Bot {gameObject.name} has no owned hexes! Cannot expand.");
            return null;
        }
        
        // Упрощенный алгоритм без сложного pathfinding для производительности
        var startHex = ownedHexes.FirstOrDefault() as IHex;
        if (startHex == null) 
        {
            Debug.LogWarning($"Bot {gameObject.name} start hex is null!");
            return null;
        }
        
        // Ищем ближайший свободный гекс
        IHex targetHex = null;
        float minDistance = float.MaxValue;
        int emptyHexesFound = 0;
        
        // Используем реальное расстояние между соседними гексами для более точного расчета
        float actualHexDistance = CalculateAverageNeighborDistance(startHex);
        float maxAllowedDistance = _config.maxTrailLength * actualHexDistance;
        
        Debug.Log($"Bot {gameObject.name} search params: maxTrailLength={_config.maxTrailLength}, cellDiameter={_grid.CellDiameter:F2}, actualHexDistance={actualHexDistance:F2}, maxDistance={maxAllowedDistance:F2}");
        
        // Сначала ищем в заданном радиусе
        foreach (var hex in _grid.AllHexes)
        {
            if (hex.State != HexState.Empty) continue;
            emptyHexesFound++;
            
            float distance = Vector3.Distance(startHex.transform.position, hex.transform.position);
            if (distance < minDistance && distance <= maxAllowedDistance)
            {
                minDistance = distance;
                targetHex = hex;
            }
        }
        
        // Если в радиусе ничего не нашли, берем просто ближайший пустой гекс
        if (targetHex == null)
        {
            Debug.LogWarning($"Bot {gameObject.name} no hexes in range {maxAllowedDistance:F2}, searching for any closest empty hex");
            minDistance = float.MaxValue;
            
            foreach (var hex in _grid.AllHexes)
            {
                if (hex.State != HexState.Empty) continue;
                
                float distance = Vector3.Distance(startHex.transform.position, hex.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    targetHex = hex;
                }
            }
        }
        
        // Если все еще ничего не нашли, пробуем любые гексы (не только Empty)
        if (targetHex == null)
        {
            Debug.LogError($"Bot {gameObject.name} no empty hexes found! Trying any hex not owned by self");
            minDistance = float.MaxValue;
            
            foreach (var hex in _grid.AllHexes)
            {
                if (hex.Owner == _character) continue; // Не идем на свои гексы
                
                float distance = Vector3.Distance(startHex.transform.position, hex.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    targetHex = hex;
                }
            }
        }
        
        Debug.Log($"Bot {gameObject.name} found {emptyHexesFound} empty hexes, target distance: {minDistance:F2}");
        
        if (targetHex == null) 
        {
            Debug.LogWarning($"Bot {gameObject.name} no suitable target hex found!");
            return null;
        }
        
        // Создаем простой прямой путь вместо A*
        var path = new List<IHex> { startHex, targetHex };
        Debug.Log($"Bot {gameObject.name} created expansion path with {path.Count} waypoints");
        return path;
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
    
    /// <summary>
    /// Вычисляет среднее расстояние до соседних гексов для более точного планирования
    /// </summary>
    private float CalculateAverageNeighborDistance(IHex startHex)
    {
        var neighbors = _grid.GetNeighbors(startHex).ToList();
        if (neighbors.Count == 0)
        {
            // Fallback на CellDiameter если нет соседей
            Debug.LogWarning($"No neighbors found for hex, using CellDiameter: {_grid.CellDiameter}");
            return _grid.CellDiameter;
        }
        
        float totalDistance = 0f;
        foreach (var neighbor in neighbors)
        {
            totalDistance += Vector3.Distance(startHex.transform.position, neighbor.transform.position);
        }
        
        float averageDistance = totalDistance / neighbors.Count;
        Debug.Log($"Calculated average neighbor distance: {averageDistance:F2} from {neighbors.Count} neighbors");
        return averageDistance;
    }
}