using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Простой и эффективный бот для io-игры с четкой логикой
/// </summary>
[RequireComponent(typeof(Mover), typeof(Conquester), typeof(IoGameBotPathProvider))]
public class IoGameBot : CharacterAbstract
{
    [Header("Bot Configuration")]
    [SerializeField] private IoGameBotConfig _config = new IoGameBotConfig();
    
    private enum BotState
    {
        Home,      // На базе, планирует следующий ход
        Expand,    // Расширяет территорию
        Return,    // Возвращается на базу
        Collect    // Подбирает монетки (если рядом)
    }
    
    private BotState _currentState = BotState.Home;
    private Mover _mover;
    private Conquester _conquester;
    private IoGameBotPathProvider _pathProvider;
    private TerritoryManager _territoryManager;
    private IHexGridProvider _grid;
    private System.Random _random;
    
    // Кэш для производительности
    private IReadOnlyCollection<IHex> _ownedHexes;
    private float _lastCacheUpdate = 0f;
    private const float CACHE_UPDATE_INTERVAL = 1f;
    
    private void Awake()
    {
        _mover = GetComponent<Mover>();
        _conquester = GetComponent<Conquester>();
        _pathProvider = GetComponent<IoGameBotPathProvider>();
        _territoryManager = FindObjectOfType<TerritoryManager>();
        _random = new System.Random(GetInstanceID());
        
        BaseInit();
    }
    
    public void Init(IHexGridProvider grid)
    {
        _grid = grid;
        StartCoroutine(BotLoop());
    }
    
    /// <summary>
    /// Основной цикл бота - простой и понятный
    /// </summary>
    private IEnumerator BotLoop()
    {
        // Рандомная задержка для распределения нагрузки
        yield return new WaitForSeconds(_random.NextSingle() * 0.5f);
        
        while (State == CharacterState.Alive)
        {
            UpdateCache();
            UpdateCurrentState();
            Move();
            
            yield return new WaitForSeconds(0.1f); // 10 FPS для ботов
        }
    }
    
    /// <summary>
    /// Обновляет кэш данных для производительности
    /// </summary>
    private void UpdateCache()
    {
        if (Time.time - _lastCacheUpdate > CACHE_UPDATE_INTERVAL)
        {
            _ownedHexes = _territoryManager.GetFixedByOwner(this);
            _lastCacheUpdate = Time.time;
        }
    }
    
    /// <summary>
    /// Обновляет состояние бота на основе ситуации
    /// </summary>
    private void UpdateCurrentState()
    {
        switch (_currentState)
        {
            case BotState.Home:
                HandleHomeState();
                break;
                
            case BotState.Expand:
                HandleExpandState();
                break;
                
            case BotState.Return:
                HandleReturnState();
                break;
                
            case BotState.Collect:
                HandleCollectState();
                break;
        }
    }
    
    private void HandleHomeState()
    {
        // Проверяем монетки рядом (высший приоритет)
        var nearbyResource = FindNearbyResource();
        if (nearbyResource != null)
        {
            SetCollectTarget(nearbyResource);
            return;
        }
        
        // Планируем расширение территории
        var expansionPath = CreateExpansionPath();
        if (expansionPath != null && expansionPath.Count > 0)
        {
            _pathProvider.SetPath(expansionPath);
            _currentState = BotState.Expand;
            
            Debug.Log($"Bot {name}: Starting expansion with {expansionPath.Count} points");
        }
    }
    
    private void HandleExpandState()
    {
        // Проверяем завершение пути
        if (IsPathCompleted())
        {
            Debug.Log($"Bot {name}: Expansion completed, returning home");
            StartReturnHome();
            return;
        }
        
        // Проверяем критические монетки рядом
        var criticalResource = FindNearbyResource();
        if (criticalResource != null && Vector3.Distance(transform.position, criticalResource.transform.position) < _config.collectRadius * 0.5f)
        {
            SetCollectTarget(criticalResource);
            return;
        }
    }
    
    private void HandleReturnState()
    {
        if (IsPathCompleted())
        {
            Debug.Log($"Bot {name}: Returned home successfully");
            _currentState = BotState.Home;
        }
    }
    
    private void HandleCollectState()
    {
        // Проверяем завершение сбора или недоступность ресурса
        var resource = FindNearbyResource();
        if (resource == null || Vector3.Distance(transform.position, resource.transform.position) > _config.collectRadius)
        {
            // Решаем что делать дальше
            if (IsOnOwnTerritory())
            {
                _currentState = BotState.Home;
            }
            else
            {
                StartReturnHome();
            }
        }
    }
    
    /// <summary>
    /// Создает путь расширения территории с учетом агрессивности
    /// </summary>
    private List<Vector3> CreateExpansionPath()
    {
        if (_ownedHexes == null || _ownedHexes.Count == 0)
        {
            Debug.LogWarning($"Bot {name}: No owned territory for expansion");
            return null;
        }
        
        // Находим границу территории
        var borderHexes = GetTerritoryBorder();
        if (borderHexes.Count == 0) return null;
        
        // Выбираем случайную точку границы
        var startHex = borderHexes[_random.Next(borderHexes.Count)];
        var startPos = startHex.transform.position;
        
        // Направление от центра территории к границе (и дальше)
        var territoryCenter = CalculateTerritoryCenter();
        var expansionDirection = (startPos - territoryCenter).normalized;
        
        // Создаем изогнутый путь
        return CreateCurvedPath(startPos, expansionDirection, _config.aggressiveness, _config.pathCurvature);
    }
    
    /// <summary>
    /// Создает изогнутый путь с заданными параметрами
    /// </summary>
    private List<Vector3> CreateCurvedPath(Vector3 start, Vector3 direction, float distance, float curvature)
    {
        var path = new List<Vector3>();
        int steps = Mathf.RoundToInt(distance * 5); // 5 точек на единицу расстояния
        
        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            
            // Базовая точка (прямая линия)
            Vector3 basePoint = start + direction * (distance * t);
            
            // Добавляем кривизну (синусоида + случайность)
            float curveOffset = Mathf.Sin(t * Mathf.PI) * curvature;
            curveOffset += (_random.NextSingle() - 0.5f) * curvature * 0.3f; // 30% случайности
            
            // Перпендикулярное направление для кривизны
            Vector3 perpendicular = new Vector3(-direction.z, 0, direction.x);
            Vector3 curvedPoint = basePoint + perpendicular * curveOffset;
            
            path.Add(curvedPoint);
        }
        
        Debug.Log($"Bot {name}: Created curved path with {path.Count} points, distance: {distance:F1}, curvature: {curvature:F1}");
        return path;
    }
    
    /// <summary>
    /// Начинает возвращение домой
    /// </summary>
    private void StartReturnHome()
    {
        if (_ownedHexes == null || _ownedHexes.Count == 0)
        {
            Debug.LogError($"Bot {name}: No home territory to return to!");
            return;
        }
        
        var nearestHome = _ownedHexes.OrderBy(h => Vector3.Distance(transform.position, h.transform.position)).First();
        var returnPath = CreateCurvedPath(
            transform.position, 
            (nearestHome.transform.position - transform.position).normalized,
            Vector3.Distance(transform.position, nearestHome.transform.position),
            _config.pathCurvature * 0.7f // Менее изогнутый возврат
        );
        
        _pathProvider.SetPath(returnPath);
        _currentState = BotState.Return;
        
        Debug.Log($"Bot {name}: Starting return home with {returnPath.Count} points");
    }
    
    /// <summary>
    /// Обрабатывает движение по текущему пути
    /// </summary>
    private void Move()
    {
        // Mover автоматически использует PathProvider
        // Здесь можно добавить дополнительную логику если нужно
    }
    
    /// <summary>
    /// Находит ближайший ресурс в радиусе сбора
    /// </summary>
    private IGrabbable FindNearbyResource()
    {
        // TODO: Реализовать поиск ресурсов в радиусе collectRadius
        // Пока заглушка
        return null;
    }
    
    /// <summary>
    /// Устанавливает цель для сбора ресурса
    /// </summary>
    private void SetCollectTarget(IGrabbable resource)
    {
        var resourcePos = (resource as MonoBehaviour)?.transform.position ?? Vector3.zero;
        var collectPath = new List<Vector3> { transform.position, resourcePos };
        _pathProvider.SetPath(collectPath);
        _currentState = BotState.Collect;
        
        Debug.Log($"Bot {name}: Collecting resource at {resourcePos}");
    }
    
    /// <summary>
    /// Проверяет завершение текущего пути
    /// </summary>
    private bool IsPathCompleted()
    {
        return _pathProvider.IsPathCompleted;
    }
    
    /// <summary>
    /// Проверяет, находится ли бот на своей территории
    /// </summary>
    private bool IsOnOwnTerritory()
    {
        if (_ownedHexes == null) return false;
        
        var currentHex = _grid.AllHexes.OrderBy(h => Vector3.Distance(transform.position, h.transform.position)).FirstOrDefault();
        return _ownedHexes.Contains(currentHex);
    }
    
    /// <summary>
    /// Получает гексы на границе территории
    /// </summary>
    private List<IHex> GetTerritoryBorder()
    {
        if (_ownedHexes == null) return new List<IHex>();
        
        var borderHexes = new List<IHex>();
        
        foreach (var hex in _ownedHexes)
        {
            var neighbors = _grid.GetNeighbors(hex);
            bool isBorder = neighbors.Any(n => n.Owner != this);
            
            if (isBorder)
            {
                borderHexes.Add(hex);
            }
        }
        
        return borderHexes;
    }
    
    /// <summary>
    /// Вычисляет центр территории
    /// </summary>
    private Vector3 CalculateTerritoryCenter()
    {
        if (_ownedHexes == null || _ownedHexes.Count == 0)
            return transform.position;
        
        Vector3 center = Vector3.zero;
        foreach (var hex in _ownedHexes)
        {
            center += hex.transform.position;
        }
        center /= _ownedHexes.Count;
        
        return center;
    }
    
    /// <summary>
    /// Обработка подбора бустеров (заглушка для будущего расширения)
    /// </summary>
    private void HandleBoosterCollected(string boosterType)
    {
        // TODO: Добавить реакции на различные бустеры
        Debug.Log($"Bot {name}: Booster collected: {boosterType}");
        
        switch (boosterType)
        {
            case "speed":
                // Увеличить скорость временно
                throw new System.NotImplementedException("Speed booster not implemented");
                
            case "shield":
                // Защита от прерывания trail
                throw new System.NotImplementedException("Shield booster not implemented");
                
            case "size":
                // Увеличить размер захватываемой области
                throw new System.NotImplementedException("Size booster not implemented");
                
            default:
                Debug.LogWarning($"Unknown booster type: {boosterType}");
                break;
        }
    }
}