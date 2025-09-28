using System.Linq;
using AI.Core;
using UnityEngine;

namespace AI.Behaviors
{
    /// <summary>
    /// Поведение исследования - расширение территории безопасным способом
    /// </summary>
    public class ExploreBehavior : BaseBehavior
    {
        public override string Name => "Explore";
        public override Priority Priority => Priority.Medium;

        private PathProvider _pathProvider;
        private IHex _targetHex;
        private float _maxExploreDistance = 8f;

        public ExploreBehavior(PathProvider pathProvider)
        {
            _pathProvider = pathProvider ?? throw new System.ArgumentNullException(nameof(pathProvider));
        }

        public override bool CanExecute(AIContext context)
        {
            if (!base.CanExecute(context)) return false;

            // Не исследуем, если в критической опасности
            if (context.IsInDanger()) return false;

            // Проверяем, есть ли безопасные места для исследования
            var safeTargets = FindSafeExplorationTargets(context);
            return safeTargets.Count > 0;
        }

        public override void OnEnter(AIContext context)
        {
            base.OnEnter(context);
            
            _targetHex = SelectBestExplorationTarget(context);
            
            if (_targetHex != null)
            {
                // Настраиваем максимальную дистанцию исследования в зависимости от личности
                _maxExploreDistance = context.Personality switch
                {
                    BotPersonality.Aggressive => 12f,     // Агрессивные идут дальше
                    BotPersonality.Defensive => 4f,       // Оборонительные близко к дому
                    BotPersonality.Opportunist => 8f,     // Оппортунисты умеренно
                    BotPersonality.Territorial => 10f,    // Территориальные любят расширяться
                    BotPersonality.Balanced => 7f,        // Сбалансированные
                    _ => 6f
                };

                // Планируем путь к цели
                var path = PlanExplorePath(context, _targetHex);
                if (path != null && path.Count > 0)
                {
                    _pathProvider.SetPath(path, context.Grid);
                    UpdateBehaviorStatus(context, $"Exploring towards {_targetHex.transform.position}\");
                }
                else
                {
                    _targetHex = null;
                    UpdateBehaviorStatus(context, \"No valid exploration path\");
                }
            }
            else
            {
                UpdateBehaviorStatus(context, \"No exploration targets found\");
            }
        }

        public override BehaviorResult Execute(AIContext context)
        {
            LastUpdateTime = Time.time;

            // Проверяем, не появилась ли угроза
            if (context.IsInDanger())
            {
                UpdateBehaviorStatus(context, \"Danger detected - aborting exploration\");
                return BehaviorResult.Failure;
            }

            // Проверяем, есть ли цель
            if (_targetHex == null)
            {
                UpdateBehaviorStatus(context, \"No target - exploration failed\");
                return BehaviorResult.Failure;
            }

            // Проверяем, достигли ли мы цели или path provider завершил движение
            if (_pathProvider.IsDone)
            {
                var distanceToTarget = Vector3.Distance(
                    context.Character.transform.position, 
                    _targetHex.transform.position
                );

                if (distanceToTarget < context.Grid.CellDiameter * 1.5f)
                {
                    UpdateBehaviorStatus(context, \"Target reached - exploration successful\");
                    return BehaviorResult.Success;
                }
                else
                {
                    // Не достигли цели, но path provider завершился - возможно, путь заблокирован
                    UpdateBehaviorStatus(context, \"Path blocked - retrying\");
                    
                    // Пытаемся найти новую цель
                    _targetHex = SelectBestExplorationTarget(context);
                    if (_targetHex != null)
                    {
                        var newPath = PlanExplorePath(context, _targetHex);
                        if (newPath != null && newPath.Count > 0)
                        {
                            _pathProvider.SetPath(newPath, context.Grid);
                            return BehaviorResult.Running;
                        }
                    }
                    
                    return BehaviorResult.Failure;
                }
            }

            // Проверяем, не идём ли мы слишком далеко от дома
            var distanceFromHome = GetDistanceFromTerritory(context);
            if (distanceFromHome > _maxExploreDistance)
            {
                UpdateBehaviorStatus(context, \"Too far from territory - returning\");
                return BehaviorResult.Failure;
            }

            // Проверяем, безопасен ли текущий путь
            if (_targetHex != null && !IsPathSafe(_targetHex.transform.position, context))
            {
                UpdateBehaviorStatus(context, \"Path became unsafe - aborting\");
                return BehaviorResult.Failure;
            }

            UpdateBehaviorStatus(context, $\"Exploring... Distance to target: {Vector3.Distance(context.Character.transform.position, _targetHex.transform.position):F1}\");
            return BehaviorResult.Running;
        }

        public override void OnExit(AIContext context)
        {
            base.OnExit(context);
            
            _targetHex = null;
            
            // Записываем результат исследования
            context.Blackboard.Set(\"last_exploration_time\", Time.time);
            context.Blackboard.Set(\"exploration_count\", context.Blackboard.GetFloat(\"exploration_count\") + 1);
        }

        private System.Collections.Generic.List<IHex> FindSafeExplorationTargets(AIContext context)
        {
            var ownedHexes = context.Character.Conquester.FixedHexes;
            var allHexes = context.Grid.AllHexes;
            var myPosition = context.Character.transform.position;

            return allHexes
                .Where(hex => !ownedHexes.Contains(hex)) // Не принадлежит нам
                .Where(hex => hex.Owner == null || hex.State != HexState.Busy) // Не занят другими
                .Where(hex => Vector3.Distance(hex.transform.position, myPosition) <= _maxExploreDistance) // В пределах досягаемости
                .Where(hex => IsPositionSafeForExploration(hex.transform.position, context)) // Безопасен
                .ToList();
        }

        private IHex SelectBestExplorationTarget(AIContext context)
        {
            var safeTargets = FindSafeExplorationTargets(context);
            if (safeTargets.Count == 0) return null;

            var ownedHexes = context.Character.Conquester.FixedHexes;
            var myPosition = context.Character.transform.position;

            // Ранжируем цели по привлекательности
            return safeTargets
                .OrderBy(hex => {
                    var score = 0f;
                    
                    // Предпочитаем места рядом с нашей территорией
                    var nearestOwnHex = FindNearestHex(hex.transform.position, ownedHexes);
                    if (nearestOwnHex != null)
                    {
                        var distanceToOwn = Vector3.Distance(hex.transform.position, nearestOwnHex.transform.position);
                        score += distanceToOwn * 0.5f; // Чем ближе к своей территории, тем лучше
                    }
                    
                    // Предпочитаем места подальше от врагов
                    var nearestEnemy = context.GetNearbyEnemies().FirstOrDefault();
                    if (nearestEnemy != null)
                    {
                        var distanceToEnemy = Vector3.Distance(hex.transform.position, nearestEnemy.transform.position);
                        score -= distanceToEnemy * 0.3f; // Чем дальше от врагов, тем лучше
                    }
                    
                    return score;
                })
                .FirstOrDefault();
        }

        private System.Collections.Generic.List<IHex> PlanExplorePath(AIContext context, IHex target)
        {
            var ownedHexes = context.Character.Conquester.FixedHexes;
            if (ownedHexes.Count == 0) return null;

            // Находим ближайший к цели гекс из нашей территории
            var startHex = FindNearestHex(target.transform.position, ownedHexes);
            if (startHex == null) return null;

            // Используем существующий Pathfinder для поиска пути
            try
            {
                return Pathfinder.AStar(
                    startHex, 
                    target, 
                    context.Grid, 
                    hex => hex.Owner == null || hex == target || ownedHexes.Contains(hex)
                );
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($\"[ExploreBehavior] Pathfinding failed: {ex.Message}\");
                return null;
            }
        }

        private bool IsPositionSafeForExploration(Vector3 position, AIContext context)
        {
            const float SAFE_DISTANCE_FROM_ENEMIES = 6f;
            
            foreach (var enemy in context.GetNearbyEnemies())
            {
                var distance = Vector3.Distance(position, enemy.transform.position);
                if (distance < SAFE_DISTANCE_FROM_ENEMIES)
                    return false;
            }
            
            return true;
        }

        private float GetDistanceFromTerritory(AIContext context)
        {
            var ownedHexes = context.Character.Conquester.FixedHexes;
            if (ownedHexes.Count == 0) return float.MaxValue;

            var myPosition = context.Character.transform.position;
            var nearestOwnHex = FindNearestHex(myPosition, ownedHexes);
            
            return nearestOwnHex != null 
                ? Vector3.Distance(myPosition, nearestOwnHex.transform.position)
                : float.MaxValue;
        }
    }
}