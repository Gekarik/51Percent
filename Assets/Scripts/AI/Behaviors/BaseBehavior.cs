using System.Collections.Generic;
using System.Linq;
using AI.Core;
using UnityEngine;

namespace AI.Behaviors
{
    /// <summary>
    /// Базовый класс для всех поведений ИИ
    /// Содержит общую логику и утилиты
    /// </summary>
    public abstract class BaseBehavior : IAIBehavior
    {
        public abstract string Name { get; }
        public abstract Priority Priority { get; }
        public float LastUpdateTime { get; set; }

        protected bool _isActive = false;
        protected float _enterTime;

        public virtual bool CanExecute(AIContext context)
        {
            // Базовая проверка - персонаж должен быть жив
            return context.Character.State == CharacterState.Alive;
        }

        public virtual void OnEnter(AIContext context)
        {
            _isActive = true;
            _enterTime = Time.time;
            LastUpdateTime = Time.time;
            
            if (AISettings.EnableDebugLogging)
                Debug.Log($\"[AI] {context.Character.name} entering behavior: {Name}\");
        }

        public abstract BehaviorResult Execute(AIContext context);

        public virtual void OnExit(AIContext context)
        {
            _isActive = false;
            
            if (AISettings.EnableDebugLogging)
            {
                var duration = Time.time - _enterTime;
                Debug.Log($\"[AI] {context.Character.name} exiting behavior: {Name} (duration: {duration:F1}s)\");
            }
        }

        /// <summary>
        /// Получить время выполнения текущего поведения
        /// </summary>
        protected float GetExecutionTime()
        {
            return _isActive ? Time.time - _enterTime : 0f;
        }

        /// <summary>
        /// Проверить, прошло ли минимальное время выполнения
        /// </summary>
        protected bool HasMinExecutionTimePassed(float minTime)
        {
            return GetExecutionTime() >= minTime;
        }

        /// <summary>
        /// Получить безопасную позицию для отступления
        /// </summary>
        protected IHex GetSafeRetreatPosition(AIContext context)
        {
            var ownedHexes = context.Character.Conquester.FixedHexes;
            if (ownedHexes.Count == 0) return null;

            var myPosition = context.Character.transform.position;
            var nearestThreat = context.GetNearestThreat();

            if (nearestThreat == null)
            {
                // Нет угрозы - возвращаемся к центру территории
                return GetTerritoryCenter(ownedHexes);
            }

            // Есть угроза - ищем самый дальний безопасный гекс
            var threatPosition = nearestThreat.transform.position;
            
            return ownedHexes
                .OrderByDescending(hex => Vector3.Distance(hex.transform.position, threatPosition))
                .FirstOrDefault() as IHex;
        }

        /// <summary>
        /// Получить центр территории
        /// </summary>
        protected IHex GetTerritoryCenter(IReadOnlyCollection<IHex> ownedHexes)
        {
            if (ownedHexes.Count == 0) return null;

            var center = Vector3.zero;
            foreach (var hex in ownedHexes)
            {
                center += hex.transform.position;
            }
            center /= ownedHexes.Count;

            // Найти ближайший гекс к центру
            return ownedHexes
                .OrderBy(hex => Vector3.Distance(hex.transform.position, center))
                .FirstOrDefault();
        }

        /// <summary>
        /// Найти ближайший гекс к позиции из коллекции
        /// </summary>
        protected IHex FindNearestHex(Vector3 position, IEnumerable<IHex> hexes)
        {
            return hexes
                .OrderBy(hex => Vector3.Distance(hex.transform.position, position))
                .FirstOrDefault();
        }

        /// <summary>
        /// Проверить, безопасен ли путь к цели
        /// </summary>
        protected bool IsPathSafe(Vector3 target, AIContext context, float safetyRadius = 5f)
        {
            var myPosition = context.Character.transform.position;
            var pathDirection = (target - myPosition).normalized;
            var pathLength = Vector3.Distance(myPosition, target);

            foreach (var enemy in context.GetNearbyEnemies())
            {
                var enemyPosition = enemy.transform.position;
                var toEnemy = enemyPosition - myPosition;
                var projectionLength = Vector3.Dot(toEnemy, pathDirection);
                
                // Враг находится на пути
                if (projectionLength > 0 && projectionLength < pathLength)
                {
                    var projection = myPosition + pathDirection * projectionLength;
                    var distanceToPath = Vector3.Distance(enemyPosition, projection);
                    
                    if (distanceToPath < safetyRadius)
                        return false;
                }
            }
            
            return true;
        }

        /// <summary>
        /// Обновить Blackboard с информацией о поведении
        /// </summary>
        protected void UpdateBehaviorStatus(AIContext context, string status)
        {
            context.Blackboard.Set(\"current_behavior\", Name);
            context.Blackboard.Set(\"behavior_status\", status);
            context.Blackboard.Set(\"behavior_execution_time\", GetExecutionTime());
        }
    }

    /// <summary>
    /// Настройки для системы ИИ
    /// </summary>
    public static class AISettings
    {
        public static bool EnableDebugLogging = true;
        public static float DefaultThinkInterval = 0.2f;
        public static float DefaultSafetyRadius = 5f;
        public static float DefaultMinExecutionTime = 1f;
    }
}