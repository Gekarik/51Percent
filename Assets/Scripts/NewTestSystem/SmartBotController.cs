using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Mover), typeof(Conquester), typeof(PathProvider))]
[RequireComponent(typeof(Grabber))]
public class SmartBotController : MonoBehaviour
{
    public enum BotState 
    { 
        Idle,           // Ожидание
        Expanding,      // Захват территории
        Returning,      // Возвращение на свою территорию
        Collecting,     // Сбор монеток/бустеров
        Attacking,      // Атака trail противника
        Fleeing,        // Убегание от опасности
        Hunting,        // Охота на конкретного противника
        Dead
    }

    [Header("Bot Configuration")]
    [SerializeField] private BotBehaviorType behaviorType = BotBehaviorType.Balanced;
    [SerializeField] private float decisionInterval = 0.5f; // Как часто принимать решения
    [SerializeField] private float sightRange = 15f;       // Радиус обзора
    [SerializeField] private float dangerRange = 8f;       // Радиус опасной зоны
    [SerializeField] private float collectRange = 20f;     // Радиус поиска монеток
    
    [Header("Debug")]
    [SerializeField] private BotState currentState = BotState.Idle;
    [SerializeField] private bool drawDebugGizmos = false;

    private Conquester _conquester;
    private PathProvider _pathProvider;
    private Grabber _grabber;
    private IHexGridProvider _grid;
    private BotPersonality _personality;
    
    // Состояние и целевые объекты
    private List<IHex> _currentPath;
    private ICharacter _targetEnemy;
    private IGrabbable _targetItem;
    private Vector3 _fleeDirection;
    private float _lastDecisionTime;
    
    // Кэш для оптимизации
    private List<ICharacter> _nearbyEnemies = new List<ICharacter>();
    private List<IGrabbable> _nearbyItems = new List<IGrabbable>();
    private CharacterAbstract[] _allCharacters;
    private float _lastScanTime;
    private const float SCAN_INTERVAL = 0.5f;

    private void Awake()
    {
        _conquester = GetComponent<Conquester>();
        _pathProvider = GetComponent<PathProvider>();
        _grabber = GetComponent<Grabber>();
        _personality = BotPersonality.CreateFromType(behaviorType);
        
        // Подписываемся на события
        _conquester.TrailInterrupted += OnTrailInterrupted;
        _grabber.ItemCollected += OnItemCollected;
    }

    private void OnDestroy()
    {
        _conquester.TrailInterrupted -= OnTrailInterrupted;
        _grabber.ItemCollected -= OnItemCollected;
    }

    public void Init(IHexGridProvider gridProvider)
    {
        _grid = gridProvider;
        StartCoroutine(BotBehaviorLoop());
        StartCoroutine(EnvironmentScanner());
    }

    private IEnumerator BotBehaviorLoop()
    {
        yield return new WaitForSeconds(Random.Range(0.5f, 1.5f)); // Начальная задержка
        
        while (currentState != BotState.Dead)
        {
            // Принимаем решения с заданным интервалом
            if (Time.time - _lastDecisionTime >= decisionInterval)
            {
                MakeDecision();
                _lastDecisionTime = Time.time;
            }

            // Выполняем текущее состояние
            ExecuteState();
            
            yield return null;
        }
    }

    private IEnumerator EnvironmentScanner()
    {
        while (currentState != BotState.Dead)
        {
            ScanEnvironment();
            yield return new WaitForSeconds(SCAN_INTERVAL);
        }
    }

    private void ScanEnvironment()
    {
        _lastScanTime = Time.time;
        
        // Находим всех персонажей на карте
        if (_allCharacters == null || Time.frameCount % 60 == 0) // Обновляем раз в секунду
        {
            _allCharacters = FindObjectsOfType<CharacterAbstract>();
        }

        // Сканируем ближайших врагов
        _nearbyEnemies.Clear();
        foreach (var character in _allCharacters)
        {
            if (character == null || character == GetComponent<ICharacter>()) 
                continue;
            
            float distance = Vector3.Distance(transform.position, character.transform.position);
            if (distance <= sightRange)
            {
                _nearbyEnemies.Add(character);
            }
        }

        // Сканируем ближайшие монетки и бустеры
        _nearbyItems.Clear();
        var allCoins = FindObjectsOfType<Coin>();
        var allBoosters = FindObjectsOfType<Booster>();
        
        foreach (var coin in allCoins)
        {
            if (coin.State == GrabbableState.Idle)
            {
                float distance = Vector3.Distance(transform.position, coin.transform.position);
                if (distance <= collectRange)
                {
                    _nearbyItems.Add(coin);
                }
            }
        }
        
        foreach (var booster in allBoosters)
        {
            if (booster.State == GrabbableState.Idle)
            {
                float distance = Vector3.Distance(transform.position, booster.transform.position);
                if (distance <= collectRange)
                {
                    _nearbyItems.Add(booster);
                }
            }
        }
    }

    private void MakeDecision()
    {
        // Проверка на опасность (приоритет выше)
        if (ShouldFlee())
        {
            currentState = BotState.Fleeing;
            return;
        }

        // Если убегали, но опасность миновала
        if (currentState == BotState.Fleeing && !IsInDanger())
        {
            currentState = BotState.Idle;
        }

        // Принимаем решение на основе текущего состояния и личности
        switch (currentState)
        {
            case BotState.Idle:
                DecideNextAction();
                break;
                
            case BotState.Expanding:
                // Проверяем, не пора ли вернуться
                if (_pathProvider.IsDone)
                {
                    StartReturning();
                }
                break;
                
            case BotState.Returning:
                if (_pathProvider.IsDone)
                {
                    CompletePath();
                    currentState = BotState.Idle;
                }
                break;
                
            case BotState.Collecting:
                // Если цель собрана или недоступна
                if (_targetItem == null || _targetItem.State != GrabbableState.Idle)
                {
                    currentState = BotState.Idle;
                }
                break;
                
            case BotState.Attacking:
                // Если цель уничтожена или далеко
                if (_targetEnemy == null || _targetEnemy.State != CharacterState.Alive ||
                    Vector3.Distance(transform.position, _targetEnemy.Transform.position) > sightRange * 1.5f)
                {
                    currentState = BotState.Idle;
                }
                break;
                
            case BotState.Hunting:
                // Проверяем, жива ли цель
                if (_targetEnemy == null || _targetEnemy.State != CharacterState.Alive)
                {
                    currentState = BotState.Idle;
                }
                break;
        }
    }

    private void DecideNextAction()
    {
        float rand = Random.value;
        
        // Решение на основе личности бота
        if (rand < _personality.greediness && _nearbyItems.Count > 0)
        {
            // Собираем монетки/бустеры
            StartCollecting();
        }
        else if (rand < _personality.aggressiveness && _nearbyEnemies.Count > 0)
        {
            // Атакуем ближайшего врага
            StartAttacking();
        }
        else if (rand < 0.7f) // Базовый шанс расширения территории
        {
            // Захватываем территорию
            StartExpanding();
        }
        else
        {
            // Ищем цель для охоты (если агрессивный)
            if (_personality.aggressiveness > 0.6f && _nearbyEnemies.Count > 0)
            {
                StartHunting();
            }
            else
            {
                StartExpanding(); // По умолчанию расширяемся
            }
        }
    }

    private void ExecuteState()
    {
        switch (currentState)
        {
            case BotState.Collecting:
                ExecuteCollecting();
                break;
                
            case BotState.Attacking:
                ExecuteAttacking();
                break;
                
            case BotState.Fleeing:
                ExecuteFleeing();
                break;
                
            case BotState.Hunting:
                ExecuteHunting();
                break;
        }
    }

    #region State Transitions

    private void StartExpanding()
    {
        int aggression = Mathf.RoundToInt(_personality.aggressiveness * 10f + 3f);
        _currentPath = new TrailPlanner().BuildTrail(_conquester.FixedHexes, _grid, aggression);
        
        if (_currentPath != null && _currentPath.Count > 1)
        {
            _pathProvider.SetPath(_currentPath, _grid);
            currentState = BotState.Expanding;
        }
    }

    private void StartReturning()
    {
        var returnPath = new TrailPlanner().BuildReturn(_currentPath, _conquester.FixedHexes, _grid);
        
        if (returnPath != null && returnPath.Count > 1)
        {
            _pathProvider.SetPath(returnPath, _grid);
            currentState = BotState.Returning;
        }
        else
        {
            currentState = BotState.Idle;
        }
    }

    private void StartCollecting()
    {
        // Находим ближайшую монетку/бустер
        _targetItem = _nearbyItems
            .Where(item => item != null && item.State == GrabbableState.Idle)
            .OrderBy(item => Vector3.Distance(transform.position, item.Transform.position))
            .FirstOrDefault();

        if (_targetItem != null)
        {
            // Строим путь к монетке
            var targetHex = GetNearestHex(_targetItem.Transform.position);
            if (targetHex != null)
            {
                var myHex = GetNearestHex(transform.position);
                var path = Pathfinder.AStar(myHex, targetHex, _grid, h => true);
                
                if (path != null && path.Count > 1)
                {
                    _pathProvider.SetPath(path, _grid);
                    currentState = BotState.Collecting;
                }
            }
        }
    }

    private void StartAttacking()
    {
        // Находим ближайшего врага с trail
        _targetEnemy = _nearbyEnemies
            .Where(e => e != null && e.State == CharacterState.Alive)
            .OrderBy(e => Vector3.Distance(transform.position, e.Transform.position))
            .FirstOrDefault();

        if (_targetEnemy != null)
        {
            currentState = BotState.Attacking;
        }
    }

    private void StartHunting()
    {
        // Выбираем самого слабого врага или с большой территорией
        _targetEnemy = _nearbyEnemies
            .Where(e => e != null && e.State == CharacterState.Alive)
            .OrderBy(e => Vector3.Distance(transform.position, e.Transform.position))
            .FirstOrDefault();

        if (_targetEnemy != null)
        {
            currentState = BotState.Hunting;
        }
    }

    private void CompletePath()
    {
        if (_currentPath != null && _currentPath.Count > 0)
        {
            _conquester.FixHexes(_currentPath);
            _currentPath = null;
        }
    }

    #endregion

    #region State Execution

    private void ExecuteCollecting()
    {
        if (_targetItem == null || _targetItem.State != GrabbableState.Idle)
        {
            currentState = BotState.Idle;
            return;
        }

        // Если путь закончен, но монетка все еще далеко, строим новый путь
        if (_pathProvider.IsDone)
        {
            float distance = Vector3.Distance(transform.position, _targetItem.Transform.position);
            if (distance > 2f)
            {
                StartCollecting(); // Пересчитываем путь
            }
            else
            {
                currentState = BotState.Idle;
            }
        }
    }

    private void ExecuteAttacking()
    {
        if (_targetEnemy == null || _targetEnemy.State != CharacterState.Alive)
        {
            currentState = BotState.Idle;
            return;
        }

        // Преследуем врага, пытаясь пересечь его trail
        Vector3 predictedPos = PredictEnemyPosition(_targetEnemy);
        var targetHex = GetNearestHex(predictedPos);
        
        if (targetHex != null)
        {
            var myHex = GetNearestHex(transform.position);
            var path = Pathfinder.AStar(myHex, targetHex, _grid, h => true);
            
            if (path != null && path.Count > 1)
            {
                _pathProvider.SetPath(path, _grid);
            }
        }
    }

    private void ExecuteFleeing()
    {
        if (!IsInDanger())
        {
            currentState = BotState.Idle;
            return;
        }

        // Находим безопасное направление
        Vector3 dangerCenter = CalculateDangerCenter();
        _fleeDirection = (transform.position - dangerCenter).normalized;
        
        // Находим безопасную точку на нашей территории
        var safeHex = FindSafeHex();
        if (safeHex != null)
        {
            var myHex = GetNearestHex(transform.position);
            var path = Pathfinder.AStar(myHex, safeHex, _grid, h => _conquester.FixedHexes.Contains(h));
            
            if (path != null && path.Count > 1)
            {
                _pathProvider.SetPath(path, _grid);
            }
        }
    }

    private void ExecuteHunting()
    {
        if (_targetEnemy == null || _targetEnemy.State != CharacterState.Alive)
        {
            currentState = BotState.Idle;
            return;
        }

        // Активно преследуем цель
        Vector3 targetPos = _targetEnemy.Transform.position;
        var targetHex = GetNearestHex(targetPos);
        
        if (targetHex != null)
        {
            var myHex = GetNearestHex(transform.position);
            var path = Pathfinder.AStar(myHex, targetHex, _grid, h => true);
            
            if (path != null && path.Count > 1)
            {
                _pathProvider.SetPath(path, _grid);
            }
        }

        // Если слишком далеко ушли от территории, возвращаемся
        if (!IsNearOwnTerritory())
        {
            currentState = BotState.Returning;
            StartReturning();
        }
    }

    #endregion

    #region Helper Methods

    private bool ShouldFlee()
    {
        if (_personality.reactiveness < 0.3f) // Не очень реактивный бот
            return false;

        return IsInDanger() && Random.value > _personality.riskTolerance;
    }

    private bool IsInDanger()
    {
        foreach (var enemy in _nearbyEnemies)
        {
            if (enemy == null || enemy.State != CharacterState.Alive)
                continue;

            float distance = Vector3.Distance(transform.position, enemy.Transform.position);
            
            // Опасность выше, если мы не на своей территории
            float effectiveDangerRange = IsOnOwnTerritory() ? dangerRange * 0.7f : dangerRange;
            
            if (distance <= effectiveDangerRange)
            {
                return true;
            }
        }
        
        return false;
    }

    private bool IsOnOwnTerritory()
    {
        var myHex = GetNearestHex(transform.position);
        return myHex != null && _conquester.FixedHexes.Contains(myHex);
    }

    private bool IsNearOwnTerritory()
    {
        var myHex = GetNearestHex(transform.position);
        if (myHex == null) return false;

        // Проверяем, есть ли своя территория в радиусе
        foreach (var ownHex in _conquester.FixedHexes)
        {
            if (_grid.Distance(myHex, ownHex) <= 5)
                return true;
        }
        
        return false;
    }

    private Vector3 CalculateDangerCenter()
    {
        Vector3 center = Vector3.zero;
        int count = 0;

        foreach (var enemy in _nearbyEnemies)
        {
            if (enemy == null || enemy.State != CharacterState.Alive)
                continue;

            float distance = Vector3.Distance(transform.position, enemy.Transform.position);
            if (distance <= dangerRange)
            {
                center += enemy.Transform.position;
                count++;
            }
        }

        return count > 0 ? center / count : transform.position + Vector3.forward * 10f;
    }

    private IHex FindSafeHex()
    {
        // Находим самую дальнюю от опасности точку на своей территории
        Vector3 dangerCenter = CalculateDangerCenter();
        
        return _conquester.FixedHexes
            .OrderByDescending(h => Vector3.Distance(h.Transform.position, dangerCenter))
            .FirstOrDefault();
    }

    private Vector3 PredictEnemyPosition(ICharacter enemy)
    {
        if (enemy == null) return Vector3.zero;
        
        // Простое предсказание на основе текущей скорости
        var mover = enemy.Transform.GetComponent<Mover>();
        if (mover != null)
        {
            float predictionTime = 1f; // Предсказываем на 1 секунду вперед
            return enemy.Transform.position + mover.PlayerSpeed * predictionTime;
        }
        
        return enemy.Transform.position;
    }

    private IHex GetNearestHex(Vector3 position)
    {
        return _grid.AllHexes
            .OrderBy(h => Vector3.Distance(h.transform.position, position))
            .FirstOrDefault();
    }

    #endregion

    #region Event Handlers

    private void OnTrailInterrupted(ICharacter owner, ICharacter interrupter)
    {
        if (interrupter == GetComponent<ICharacter>())
            return;

        // Если наш trail прервали, переходим в режим убегания
        if (owner == GetComponent<ICharacter>())
        {
            currentState = BotState.Fleeing;
        }
    }

    private void OnItemCollected(IGrabbable item)
    {
        // Можно добавить логику реакции на сбор предмета
        // Например, становиться более агрессивным после сбора бустера
        if (item is Booster)
        {
            _personality.aggressiveness = Mathf.Min(1f, _personality.aggressiveness + 0.2f);
            _personality.riskTolerance = Mathf.Min(1f, _personality.riskTolerance + 0.1f);
        }
    }

    #endregion

    #region Configuration Methods

    public void SetBehaviorType(BotBehaviorType newType)
    {
        behaviorType = newType;
        _personality = BotPersonality.CreateFromType(newType);
    }

    public void SetSightRange(float range)
    {
        sightRange = Mathf.Max(1f, range);
    }

    public void SetDangerRange(float range)
    {
        dangerRange = Mathf.Max(1f, range);
    }

    public void SetCollectRange(float range)
    {
        collectRange = Mathf.Max(1f, range);
    }

    public void SetDecisionInterval(float interval)
    {
        decisionInterval = Mathf.Max(0.1f, interval);
    }

    public void AdjustPersonality(float aggressivenessModifier, float greedinessModifier, float riskModifier)
    {
        _personality.aggressiveness = Mathf.Clamp01(_personality.aggressiveness + aggressivenessModifier);
        _personality.greediness = Mathf.Clamp01(_personality.greediness + greedinessModifier);
        _personality.riskTolerance = Mathf.Clamp01(_personality.riskTolerance + riskModifier);
    }

    public BotBehaviorType GetBehaviorType()
    {
        return behaviorType;
    }

    public BotState GetCurrentState()
    {
        return currentState;
    }

    #endregion

    #region Debug

    private void OnDrawGizmosSelected()
    {
        if (!drawDebugGizmos) return;

        // Радиус обзора
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        // Радиус опасности
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, dangerRange);

        // Радиус сбора
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, collectRange);

        // Текущая цель
        if (_targetEnemy != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, _targetEnemy.Transform.position);
        }

        if (_targetItem != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, _targetItem.Transform.position);
        }
    }

    #endregion
}