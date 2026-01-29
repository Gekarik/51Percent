using System.Collections.Generic;
using UnityEngine;

public class EnemyBrain : VectorProviderComponent
{
    private const float StrategyHysteresis = 1f;
    private const float DirectionSmoothSpeed = 5f; // рад/с — скорость поворота направления

    [SerializeField] private BotPersonalitySettings _personality;
    [SerializeField] private LayerMask _collectibleLayer;
    [SerializeField] private LayerMask _characterLayer;

    private IHexGridProvider _grid;
    private Conqueror _conqueror;
    private ICharacter _character;
    private BotContext _context;

    private List<IBotStrategy> _strategies;
    private IBotStrategy _currentStrategy;
    private ExpandTerritoryStrategy _expandStrategy;

    private Vector3 _targetPosition;
    private Vector3 _moveDirection;
    private float _nextDecisionTime;
    private bool _initialized;

    private readonly Collider[] _overlapBuffer = new Collider[16];

    public void Init(IHexGridProvider grid, BotPersonalitySettings personality = null)
    {
        _grid = grid;
        _conqueror = GetComponent<Conqueror>();
        _character = GetComponent<ICharacter>();

        if (personality != null)
            _personality = personality;

        InitContext();
        InitStrategies();

        _targetPosition = transform.position;
        _moveDirection = Vector3.forward;
        _initialized = true;
    }

    private void InitContext()
    {
        _context = new BotContext
        {
            BotTransform = transform,
            Grid = _grid,
            Conqueror = _conqueror,
            Character = _character,
            Personality = _personality
        };
    }

    private void InitStrategies()
    {
        _expandStrategy = new ExpandTerritoryStrategy();

        _strategies = new List<IBotStrategy>
        {
            new AvoidDangerStrategy(),    // Высокий приоритет при опасности
            new ReturnHomeStrategy(),      // Важно вернуться если trail длинный
            new AttackEnemyStrategy(),     // Атака уязвимых врагов
            new CollectCoinStrategy(),     // Сбор монет
            _expandStrategy               // Расширение территории (default)
        };
    }

    public override Vector3 GetMoveDirection()
    {
        if (!_initialized || _personality == null)
            return Vector3.zero;

        if (Time.time >= _nextDecisionTime)
        {
            UpdateContext();
            SelectStrategy();
            UpdateTarget();

            _nextDecisionTime = Time.time + _personality.ReactionTime;
        }

        // Плавный поворот к цели
        Vector3 toTarget = _targetPosition - transform.position;
        toTarget.y = 0f;

        if (toTarget.magnitude > 0.3f)
        {
            Vector3 desired = toTarget.normalized;
            float maxTurn = DirectionSmoothSpeed * Time.deltaTime;
            _moveDirection = Vector3.RotateTowards(_moveDirection, desired, maxTurn, 0f);
        }

        _moveDirection.y = 0f;
        _context.CurrentDirection = _moveDirection;

        return _moveDirection.sqrMagnitude > 0.01f ? _moveDirection.normalized : Vector3.forward;
    }

    private void UpdateContext()
    {
        _context.Clear();
        _context.CurrentHex = _grid.GetHexAt(transform.position);

        DetectNearbyCoins();
        DetectNearbyEnemies();
        DetectDangerousTrails();
    }

    private void DetectNearbyCoins()
    {
        int count = Physics.OverlapSphereNonAlloc(
            transform.position,
            _personality.DetectionRadius,
            _overlapBuffer,
            _collectibleLayer
        );

        for (int i = 0; i < count; i++)
        {
            if (_overlapBuffer[i] != null && _overlapBuffer[i].TryGetComponent<ICollectible>(out _))
            {
                _context.NearbyCoins.Add(_overlapBuffer[i].transform);
            }
        }
    }

    private void DetectNearbyEnemies()
    {
        int count = Physics.OverlapSphereNonAlloc(
            transform.position,
            _personality.DetectionRadius,
            _overlapBuffer,
            _characterLayer
        );

        for (int i = 0; i < count; i++)
        {
            if (_overlapBuffer[i] != null &&
                _overlapBuffer[i].TryGetComponent<ICharacter>(out var character))
            {
                if (character != _character && character.State == CharacterState.Alive)
                {
                    _context.NearbyEnemies.Add(character);
                }
            }
        }
    }

    private void DetectDangerousTrails()
    {
        if (_context.CurrentHex == null)
            return;

        var coord = _grid.GetCoord(_context.CurrentHex);
        int searchRadius = Mathf.CeilToInt(_personality.DetectionRadius / 2f);

        foreach (var hex in _grid.GetHexesInRadius(coord, searchRadius))
        {
            if (hex.State == HexState.PartOfTrail && hex.Owner != _character)
            {
                _context.DangerousTrails.Add(hex);
            }
        }
    }

    private void SelectStrategy()
    {
        // Если текущая стратегия блокирует переключение - оставляем её
        if (_currentStrategy != null && _currentStrategy.IsBlocking(_context))
        {
            return;
        }

        // Utility и приоритет текущей стратегии
        float? currentUtility = _currentStrategy?.Evaluate(_context);
        int currentPriority = _currentStrategy != null ? GetStrategyPriority(_currentStrategy.Type) : -1;

        // Выбираем стратегию с максимальным utility (с учётом приоритетов)
        IBotStrategy bestStrategy = null;
        float bestUtility = float.MinValue;

        foreach (var strategy in _strategies)
        {
            // Не переключаемся на стратегию с более низким приоритетом,
            // пока текущая ещё применима
            if (currentUtility.HasValue && GetStrategyPriority(strategy.Type) < currentPriority)
                continue;

            float? utility = strategy.Evaluate(_context);

            if (!utility.HasValue)
                continue;

            // Гистерезис: текущая стратегия получает бонус, чтобы избежать дёрганья
            float adjustedUtility = utility.Value;
            if (strategy == _currentStrategy && currentUtility.HasValue)
                adjustedUtility += StrategyHysteresis;

            if (adjustedUtility > bestUtility)
            {
                bestUtility = adjustedUtility;
                bestStrategy = strategy;
            }
        }

        // Переключаем стратегию если нашли лучшую
        if (bestStrategy != null && bestStrategy != _currentStrategy)
        {
            Debug.Log($"[{transform.name}] Strategy: {_currentStrategy?.Type} → {bestStrategy.Type} " +
                      $"(utility={bestUtility:F1})");

            _currentStrategy?.OnDeactivate(_context);
            _currentStrategy = bestStrategy;
            _currentStrategy.OnActivate(_context);

            // Сбрасываем направление расширения при смене стратегии
            if (_currentStrategy != _expandStrategy)
            {
                _expandStrategy.ResetDirection();
            }
        }

        // Fallback на расширение
        if (_currentStrategy == null)
        {
            _currentStrategy = _expandStrategy;
            _currentStrategy.OnActivate(_context);
        }
    }

    private void UpdateTarget()
    {
        if (_currentStrategy != null)
        {
            _targetPosition = _currentStrategy.GetTargetPosition(_context);
        }
    }

    private static int GetStrategyPriority(StrategyType type)
    {
        return type switch
        {
            StrategyType.AvoidDanger => 3,
            StrategyType.ReturnHome => 2,
            StrategyType.AttackEnemy => 1,
            StrategyType.CollectCoin => 1,
            StrategyType.ExpandTerritory => 0,
            _ => 0
        };
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!_initialized || _personality == null)
            return;

        // Радиус обнаружения
        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, _personality.DetectionRadius);

        // Цвет зависит от текущей стратегии
        Gizmos.color = GetStrategyColor();
        Gizmos.DrawLine(transform.position, _targetPosition);
        Gizmos.DrawWireSphere(_targetPosition, 0.3f);
    }

    private Color GetStrategyColor()
    {
        if (_currentStrategy == null)
            return Color.gray;

        return _currentStrategy.Type switch
        {
            StrategyType.ExpandTerritory => Color.cyan,
            StrategyType.ReturnHome => Color.green,
            StrategyType.CollectCoin => Color.yellow,
            StrategyType.AttackEnemy => Color.red,
            StrategyType.AvoidDanger => Color.magenta,
            _ => Color.gray
        };
    }
#endif
}
