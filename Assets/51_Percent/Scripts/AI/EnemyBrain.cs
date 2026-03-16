using System.Collections.Generic;
using UnityEngine;

public class EnemyBrain : VectorProviderComponent
{
    private enum BotState { Expanding, Returning, Attacking, Collecting }

    private const float DirectionSmoothSpeed = 5f;
    private const float TrailThreatRadius = 3.5f;
    private const int ScanDirections = 8;
    private const int ScanSteps = 5;
    private const float ScanStepDistance = 2f;
    private const float EnemyDirectionPenalty = 10f;
    private const float ArrivalThreshold = 2f;

    [SerializeField] private BotPersonalitySettings _personality;
    [SerializeField] private float _expansionDangerRadius = 7f;

    private IHexGridProvider _grid;
    private IReadOnlyList<ICharacter> _allCharacters;
    private ICollectibleRegistry _collectibleRegistry;
    private Conqueror _conqueror;
    private ICharacter _character;

    private BotState _state;
    private Vector3 _targetPosition;
    private Vector3 _moveDirection;
    private float _nextThinkTime;
    private bool _initialized;

    // Expansion
    private Vector3 _expansionMidpoint;
    private Vector3 _expansionEndpoint;
    private bool _expansionPlanned;
    private bool _reachedMidpoint;

    // Attack
    private ICharacter _attackTarget;
    private Vector3 _failedAttackPosition;
    private bool _hasFailedAttackRepulsor;

    // Collect
    private ICollectible _collectibleTarget;

    public void Init(IHexGridProvider grid, IReadOnlyList<ICharacter> allCharacters,
        ICollectibleRegistry collectibleRegistry, BotPersonalitySettings personality = null, int botIndex = 0)
    {
        _grid = grid;
        _allCharacters = allCharacters;
        _collectibleRegistry = collectibleRegistry;
        _conqueror = GetComponent<Conqueror>();
        _character = GetComponent<ICharacter>();

        if (personality != null)
            _personality = personality;

        _state = BotState.Expanding;
        _targetPosition = transform.position;
        _moveDirection = Vector3.forward;
        _nextThinkTime = Time.time + botIndex * 0.15f;
        _initialized = true;
    }

    public override Vector3 GetMoveDirection()
    {
        if (!_initialized || _personality == null)
            return Vector3.zero;

        if (Time.time >= _nextThinkTime)
        {
            Think();
            _nextThinkTime = Time.time + _personality.ReactionTime;
        }

        Vector3 toTarget = _targetPosition - transform.position;
        toTarget.y = 0f;

        if (toTarget.magnitude > 0.3f)
        {
            Vector3 desired = toTarget.normalized;
            float maxTurn = DirectionSmoothSpeed * Time.deltaTime;
            _moveDirection = Vector3.RotateTowards(_moveDirection, desired, maxTurn, 0f);
        }

        _moveDirection.y = 0f;
        return _moveDirection.sqrMagnitude > 0.01f ? _moveDirection.normalized : Vector3.forward;
    }

    // ── FSM ──────────────────────────────────────────────────────────────

    private void Think()
    {
        switch (_state)
        {
            case BotState.Expanding:   ThinkExpanding();   break;
            case BotState.Returning:   ThinkReturning();   break;
            case BotState.Attacking:   ThinkAttacking();   break;
            case BotState.Collecting:  ThinkCollecting();  break;
        }
    }

    private void ThinkExpanding()
    {
        bool hasTrail = _conqueror.TrailHexes.Count > 0;

        if (hasTrail && (IsTrailThreatened() || _conqueror.TrailHexes.Count >= _personality.MaxTrailLength))
        {
            EnterReturning();
            return;
        }

        if (!hasTrail)
        {
            var attackTarget = FindAttackTarget();
            if (attackTarget != null)
            {
                EnterAttacking(attackTarget);
                return;
            }

            var collectible = FindNearestCollectible();
            if (collectible != null)
            {
                EnterCollecting(collectible);
                return;
            }
        }

        if (!_expansionPlanned)
            PlanExpansion();

        _targetPosition = GetExpansionTarget();
    }

    private void ThinkReturning()
    {
        if (_conqueror.TrailHexes.Count == 0)
        {
            EnterExpanding();
            return;
        }

        _targetPosition = GetNearestHomePosition();
    }

    private void ThinkAttacking()
    {
        if (_attackTarget == null ||
            _attackTarget.State != CharacterState.Alive ||
            !_attackTarget.HasActiveTrail)
        {
            _failedAttackPosition = _attackTarget?.Transform.position ?? transform.position;
            _hasFailedAttackRepulsor = true;
            EnterReturning();
            return;
        }

        if (_character.HasActiveTrail && IsTrailThreatened())
        {
            EnterReturning();
            return;
        }

        _targetPosition = GetAttackPosition();
    }

    private void ThinkCollecting()
    {
        if (_collectibleTarget == null || _collectibleTarget.State != GrabbableState.Idle)
        {
            EnterExpanding();
            return;
        }

        if (_conqueror.TrailHexes.Count > 0 && IsTrailThreatened())
        {
            EnterReturning();
            return;
        }

        _targetPosition = _collectibleTarget.Transform.position;
    }

    // ── Переходы ─────────────────────────────────────────────────────────

    private void EnterExpanding()
    {
        _state = BotState.Expanding;
        _expansionPlanned = false;
        _reachedMidpoint = false;
        _attackTarget = null;
        _collectibleTarget = null;
    }

    private void EnterReturning()
    {
        _state = BotState.Returning;
        _attackTarget = null;
        _collectibleTarget = null;
    }

    private void EnterAttacking(ICharacter target)
    {
        _state = BotState.Attacking;
        _attackTarget = target;
    }

    private void EnterCollecting(ICollectible collectible)
    {
        _state = BotState.Collecting;
        _collectibleTarget = collectible;
    }

    // ── Сенсоры ──────────────────────────────────────────────────────────

    private bool IsTrailThreatened()
    {
        var trail = _conqueror.TrailHexes;
        if (trail.Count == 0) return false;

        float threatSq = TrailThreatRadius * TrailThreatRadius;
        float detSq = _personality.DetectionRadius * _personality.DetectionRadius;

        foreach (var character in _allCharacters)
        {
            if (character == null || character == _character) continue;
            if (character.State != CharacterState.Alive) continue;

            float enemyDistSq = (character.Transform.position - transform.position).sqrMagnitude;
            if (enemyDistSq > detSq) continue;

            // Проверяем только последние хексы трейла (рядом с ботом)
            int from = Mathf.Max(0, trail.Count - 5);
            for (int i = from; i < trail.Count; i++)
            {
                var hex = trail[i];
                if (hex?.Transform == null) continue;

                float distSq = (character.Transform.position - hex.Transform.position).sqrMagnitude;
                if (distSq < threatSq) return true;
            }
        }

        return false;
    }

    private ICharacter FindAttackTarget()
    {
        float detSq = _personality.DetectionRadius * _personality.DetectionRadius;
        ICharacter best = null;
        float bestDist = float.MaxValue;

        foreach (var character in _allCharacters)
        {
            if (character == null || character == _character) continue;
            if (character.State != CharacterState.Alive) continue;
            if (!character.HasActiveTrail) continue;

            float distSq = (character.Transform.position - transform.position).sqrMagnitude;
            if (distSq > detSq) continue;

            float dist = Mathf.Sqrt(distSq);

            // Агрессия определяет, насколько далёкую цель бот готов атаковать
            float maxAttackDist = _personality.DetectionRadius * _personality.Aggression;
            if (dist > maxAttackDist) continue;

            if (dist < bestDist)
            {
                bestDist = dist;
                best = character;
            }
        }

        return best;
    }

    private ICollectible FindNearestCollectible()
    {
        if (_collectibleRegistry == null) return null;

        float maxDist = _personality.DetectionRadius * _personality.Greed;
        float maxDistSq = maxDist * maxDist;

        ICollectible best = null;
        float bestDistSq = float.MaxValue;

        foreach (var collectible in _collectibleRegistry.ActiveCollectibles)
        {
            if (collectible == null || collectible.State != GrabbableState.Idle) continue;

            float distSq = (collectible.Transform.position - transform.position).sqrMagnitude;
            if (distSq > maxDistSq) continue;

            if (distSq < bestDistSq)
            {
                bestDistSq = distSq;
                best = collectible;
            }
        }

        return best;
    }

    // ── Расширение ───────────────────────────────────────────────────────

    private void PlanExpansion()
    {
        Vector3 outDir = FindBestExpansionDirection();
        float sign = Random.value > 0.5f ? 1f : -1f;
        Vector3 latDir = Quaternion.Euler(0f, sign * 90f, 0f) * outDir;

        float halfLength = _personality.MaxTrailLength * 0.4f * ScanStepDistance;

        _expansionMidpoint = FindFarthestValidPoint(transform.position, outDir, halfLength);
        _expansionEndpoint = FindFarthestValidPoint(_expansionMidpoint, latDir, halfLength);

        _reachedMidpoint = false;
        _expansionPlanned = true;
        _hasFailedAttackRepulsor = false;
    }

    private Vector3 GetExpansionTarget()
    {
        if (!_reachedMidpoint)
        {
            if ((transform.position - _expansionMidpoint).sqrMagnitude < ArrivalThreshold * ArrivalThreshold)
                _reachedMidpoint = true;
            else
                return _expansionMidpoint;
        }

        return _expansionEndpoint;
    }

    // Находит самую дальнюю допустимую точку вдоль направления до границы карты
    private Vector3 FindFarthestValidPoint(Vector3 from, Vector3 direction, float maxDistance)
    {
        Vector3 best = from;
        for (float d = ScanStepDistance; d <= maxDistance; d += ScanStepDistance)
        {
            Vector3 candidate = from + direction * d;
            if (_grid.GetHexAt(candidate) != null)
                best = candidate;
            else
                break;
        }
        return best;
    }

    private Vector3 FindBestExpansionDirection()
    {
        Vector3 territoryCenter = GetTerritoryCenter();
        Vector3 fromCenter = transform.position - territoryCenter;
        fromCenter.y = 0f;
        Vector3 centerDir = fromCenter.magnitude > 0.5f ? fromCenter.normalized : Vector3.zero;

        Vector3 bestDir = Vector3.forward;
        float bestScore = float.MinValue;
        float caution = 1f - _personality.Aggression;

        for (int i = 0; i < ScanDirections; i++)
        {
            float angle = i * (360f / ScanDirections) * Mathf.Deg2Rad;
            Vector3 candidate = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));

            float score = 0f;
            float dangerRadiusSq = _expansionDangerRadius * _expansionDangerRadius;

            for (int step = 1; step <= ScanSteps; step++)
            {
                Vector3 scanPos = transform.position + candidate * step * ScanStepDistance;

                IHex hex = _grid.GetHexAt(scanPos);
                if (hex == null) { score -= 10f; break; }
                if (hex.State == HexState.Empty) score += 3f;
                else if (hex.State == HexState.Busy && hex.Owner != _character) score += 2f;

                // Потенциальное поле отталкивания: штраф пропорционален близости врага к каждой точке пути
                if (_allCharacters != null)
                {
                    foreach (var character in _allCharacters)
                    {
                        if (character == null || character == _character) continue;
                        if (character.State != CharacterState.Alive) continue;

                        float distSq = (character.Transform.position - scanPos).sqrMagnitude;
                        if (distSq >= dangerRadiusSq) continue;

                        score -= (1f - distSq / dangerRadiusSq) * EnemyDirectionPenalty * caution;
                    }
                }

                // Усиленный репульсор на месте провалившейся атаки
                if (_hasFailedAttackRepulsor)
                {
                    float distSq = (_failedAttackPosition - scanPos).sqrMagnitude;
                    if (distSq < dangerRadiusSq)
                        score -= (1f - distSq / dangerRadiusSq) * EnemyDirectionPenalty * 2f;
                }
            }

            if (centerDir.sqrMagnitude > 0.01f)
                score += Vector3.Dot(candidate, centerDir) * 0.5f;

            if (score > bestScore)
            {
                bestScore = score;
                bestDir = candidate;
            }
        }

        return Quaternion.Euler(0f, Random.Range(-20f, 20f), 0f) * bestDir;
    }

    // ── Возврат домой ────────────────────────────────────────────────────

    private Vector3 GetNearestHomePosition()
    {
        var territory = _conqueror.FixedHexes;
        if (territory == null || territory.Count == 0)
            return transform.position;

        IHex nearest = null;
        float nearestSq = float.MaxValue;

        foreach (var hex in territory)
        {
            if (hex?.Transform == null) continue;
            float sq = (transform.position - hex.Transform.position).sqrMagnitude;
            if (sq < nearestSq)
            {
                nearestSq = sq;
                nearest = hex;
            }
        }

        return nearest?.Transform.position ?? transform.position;
    }

    // ── Атака ────────────────────────────────────────────────────────────

    private Vector3 GetAttackPosition()
    {
        IHex enemyHex = _grid.GetHexAt(_attackTarget.Transform.position);
        if (enemyHex != null)
        {
            var coord = _grid.GetCoord(enemyHex);
            int radius = Mathf.CeilToInt(_personality.DetectionRadius * 0.5f);

            IHex nearestTrail = null;
            float nearestSq = float.MaxValue;

            foreach (var hex in _grid.GetHexesInRadius(coord, radius))
            {
                if (hex.State != HexState.PartOfTrail || hex.Owner != _attackTarget) continue;
                float sq = (transform.position - hex.Transform.position).sqrMagnitude;
                if (sq < nearestSq)
                {
                    nearestSq = sq;
                    nearestTrail = hex;
                }
            }

            if (nearestTrail?.Transform != null)
                return nearestTrail.Transform.position;
        }

        return _attackTarget.Transform.position;
    }

    // ── Вспомогательное ──────────────────────────────────────────────────

    private Vector3 GetTerritoryCenter()
    {
        var territory = _conqueror.FixedHexes;
        if (territory == null || territory.Count == 0)
            return transform.position;

        Vector3 sum = Vector3.zero;
        int count = 0;
        foreach (var hex in territory)
        {
            if (hex?.Transform == null) continue;
            sum += hex.Transform.position;
            count++;
        }

        return count > 0 ? sum / count : transform.position;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!_initialized || _personality == null) return;

        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, _personality.DetectionRadius);

        Gizmos.color = _state switch
        {
            BotState.Expanding  => Color.cyan,
            BotState.Returning  => Color.green,
            BotState.Attacking  => Color.red,
            BotState.Collecting => Color.yellow,
            _ => Color.gray
        };

        Gizmos.DrawLine(transform.position, _targetPosition);
        Gizmos.DrawWireSphere(_targetPosition, 0.3f);

        if (_state == BotState.Expanding && _expansionPlanned)
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.5f);
            Gizmos.DrawWireSphere(_expansionMidpoint, 0.4f);
            Gizmos.DrawWireSphere(_expansionEndpoint, 0.4f);
            Gizmos.DrawLine(_expansionMidpoint, _expansionEndpoint);
        }

        Gizmos.color = new Color(1f, 0.4f, 0f, 0.15f);
        Gizmos.DrawWireSphere(transform.position, _expansionDangerRadius);

        if (_hasFailedAttackRepulsor)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
            Gizmos.DrawWireSphere(_failedAttackPosition, _expansionDangerRadius);
        }
    }
#endif
}
