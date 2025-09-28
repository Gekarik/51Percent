using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AI.Core;
using AI.Services;

namespace AI.Behaviors
{
    /// <summary>
    /// Упрощённое поведение исследования с интеграцией AI сервисов
    /// Заменяет старый ExploreBehavior (256 строк -> 120 строк)
    /// </summary>
    public class ExploreBehavior : BaseBehavior
    {
        public override string Name => "Explore";
        public override Priority Priority => Priority.Medium;

        private readonly PathProvider _pathProvider;
        private IHex _targetHex;
        private float _maxExploreDistance = 8f;
        private float _safetyRadius = 6f;

        public ExploreBehavior(PathProvider pathProvider)
        {
            _pathProvider = pathProvider ?? throw new System.ArgumentNullException(nameof(pathProvider));
        }

        public override bool CanExecute(AIContext context)
        {
            if (!base.CanExecute(context)) return false;

            // Не исследуем в опасности
            if (context.IsInDanger()) return false;

            // Проверяем доступность сервисов
            if (!AreServicesReady()) return false;

            // Ищем безопасные цели для исследования
            var targets = FindSafeTargets(context);
            return targets.Count > 0;
        }

        public override void OnEnter(AIContext context)
        {
            base.OnEnter(context);

            // Настраиваем параметры под личность
            ConfigureForPersonality(context.Personality);

            // Выбираем лучшую цель
            _targetHex = SelectBestTarget(context);

            if (_targetHex != null)
            {
                // Планируем безопасный путь
                var path = PlanSafePath(context, _targetHex);
                if (path != null && path.Count > 0)
                {
                    _pathProvider.SetPath(path, context.Grid);
                    UpdateBehaviorStatus(context, $"Exploring to {_targetHex.transform.position}");
                }
                else
                {
                    _targetHex = null;
                    UpdateBehaviorStatus(context, "No safe path found");
                }
            }
            else
            {
                UpdateBehaviorStatus(context, "No exploration targets");
            }
        }

        public override BehaviorResult Execute(AIContext context)
        {
            LastUpdateTime = Time.time;

            // Проверяем опасность
            if (context.IsInDanger())
            {
                UpdateBehaviorStatus(context, "Danger detected, stopping exploration");
                return BehaviorResult.Failure;
            }

            // Проверяем доступность сервисов
            if (!AreServicesReady())
            {
                UpdateBehaviorStatus(context, "AI services not ready");
                return BehaviorResult.Failure;
            }

            // Проверяем прогресс движения
            if (_pathProvider.IsDone)
            {
                // Достигли цели
                if (_targetHex != null)
                {
                    UpdateBehaviorStatus(context, $"Reached target {_targetHex.transform.position}");
                    return BehaviorResult.Success;
                }
            }

            // Проверяем, не застряли ли мы
            if (_pathProvider.IsStuck)
            {
                UpdateBehaviorStatus(context, "Path blocked, searching new target");
                
                // Ищем новую цель
                _targetHex = SelectBestTarget(context);
                if (_targetHex != null)
                {
                    var newPath = PlanSafePath(context, _targetHex);
                    if (newPath != null && newPath.Count > 0)
                    {
                        _pathProvider.SetPath(newPath, context.Grid);
                        return BehaviorResult.Running;
                    }
                }
                
                return BehaviorResult.Failure;
            }

            return BehaviorResult.Running;
        }

        public override void OnExit(AIContext context)
        {
            base.OnExit(context);
            _targetHex = null;
            _pathProvider.ClearPath();
        }

        #region Private Methods

        private bool AreServicesReady()
        {
            return AIServiceContainer.Instance?.AreServicesReady() ?? false;
        }

        private void ConfigureForPersonality(BotPersonality personality)
        {
            switch (personality)
            {
                case BotPersonality.Aggressive:
                    _maxExploreDistance = 12f;
                    _safetyRadius = 4f; // Менее осторожные
                    break;
                case BotPersonality.Defensive:
                    _maxExploreDistance = 4f;
                    _safetyRadius = 8f; // Очень осторожные
                    break;
                case BotPersonality.Opportunist:
                    _maxExploreDistance = 8f;
                    _safetyRadius = 6f;
                    break;
                case BotPersonality.Territorial:
                    _maxExploreDistance = 10f;
                    _safetyRadius = 5f;
                    break;
                default:
                    _maxExploreDistance = 7f;
                    _safetyRadius = 6f;
                    break;
            }
        }

        private List<IHex> FindSafeTargets(AIContext context)
        {
            var ownedHexes = context.GetOwnedHexes();
            var nearbyEnemies = context.GetNearbyEnemies();

            // Используем TerritoryAnalysisService для поиска лучших областей расширения
            var expansionTargets = AIServiceContainer.TerritoryAnalysis
                .FindBestExpansionArea(ownedHexes, context.Grid, nearbyEnemies);

            // Фильтруем по безопасности и дистанции
            return expansionTargets
                .Where(hex => IsTargetSafe(hex, context, nearbyEnemies))
                .Where(hex => IsWithinExploreDistance(hex, ownedHexes))
                .ToList();
        }

        private IHex SelectBestTarget(AIContext context)
        {
            var targets = FindSafeTargets(context);
            if (targets.Count == 0) return null;

            var ownedHexes = context.GetOwnedHexes();
            var characterPosition = context.Character.transform.position;

            // Выбираем ближайшую безопасную цель
            return AIServiceContainer.BehaviorUtils
                .FindNearestHex(characterPosition, targets);
        }

        private List<IHex> PlanSafePath(AIContext context, IHex target)
        {
            var currentHex = AIServiceContainer.BehaviorUtils
                .FindNearestHex(context.Character.transform.position, context.Grid.AllHexes);

            if (currentHex == null) return null;

            var nearbyEnemies = context.GetNearbyEnemies();

            // Используем PathfindingService для безопасного пути
            return AIServiceContainer.Pathfinding
                .FindSafePath(currentHex, target, context.Grid, nearbyEnemies, _safetyRadius);
        }

        private bool IsTargetSafe(IHex hex, AIContext context, List<ICharacter> enemies)
        {
            var hexPosition = hex.transform.position;
            
            // Используем BehaviorUtilsService для проверки безопасности
            return AIServiceContainer.BehaviorUtils
                .IsPositionSafeForExploration(hexPosition, enemies, _safetyRadius);
        }

        private bool IsWithinExploreDistance(IHex hex, System.Collections.Generic.IReadOnlyCollection<IHex> ownedHexes)
        {
            var hexPosition = hex.transform.position;
            
            // Используем BehaviorUtilsService для расчёта дистанции
            var distanceFromTerritory = AIServiceContainer.BehaviorUtils
                .GetDistanceFromTerritory(hexPosition, ownedHexes);

            return distanceFromTerritory <= _maxExploreDistance;
        }

        #endregion

        #region Debug Support

        public override string GetDebugInfo()
        {
            var info = base.GetDebugInfo();
            info += $"\n  Target: {(_targetHex?.transform.position.ToString() ?? "None")}";
            info += $"\n  Max Distance: {_maxExploreDistance:F1}";
            info += $"\n  Safety Radius: {_safetyRadius:F1}";
            info += $"\n  Path Progress: {(_pathProvider?.GetPathProgress() ?? 0f):P0}";
            info += $"\n  Services Ready: {AreServicesReady()}";
            return info;
        }

        #endregion
    }
}