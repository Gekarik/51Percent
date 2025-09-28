using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AI.Services
{
    /// <summary>
    /// Сервис утилит для поведений - заменяет статический BehaviorUtils
    /// Оптимизирован для WebGL без утечек памяти
    /// </summary>
    public class BehaviorUtilsService : MonoBehaviour
    {
        [Header("Performance Settings")]
        [SerializeField] private int _maxCacheSize = 50;
        [SerializeField] private float _cacheCleanupInterval = 30f;

        // Кэш для оптимизации (с ограничением размера для WebGL)
        private Dictionary<int, IHex> _nearestHexCache;
        private float _lastCleanupTime;

        private void Awake()
        {
            _nearestHexCache = new Dictionary<int, IHex>();
            _lastCleanupTime = Time.time;
        }

        private void Update()
        {
            // Периодическая очистка кэша для предотвращения утечек памяти
            if (Time.time - _lastCleanupTime > _cacheCleanupInterval)
            {
                CleanupCache();
                _lastCleanupTime = Time.time;
            }
        }

        /// <summary>
        /// Найти ближайший гекс к позиции из коллекции
        /// </summary>
        public IHex FindNearestHex(Vector3 position, IEnumerable<IHex> hexes)
        {
            if (hexes == null) return null;

            var hexList = hexes as IList<IHex> ?? hexes.ToList();
            if (hexList.Count == 0) return null;

            // Создаём хэш для кэширования
            var hash = GetPositionHash(position, hexList.Count);
            
            if (_nearestHexCache.TryGetValue(hash, out var cachedHex) && 
                hexList.Contains(cachedHex))
            {
                return cachedHex;
            }

            // Ищем ближайший
            var nearest = hexList
                .OrderBy(hex => Vector3.Distance(hex.transform.position, position))
                .FirstOrDefault();

            // Кэшируем результат (с ограничением размера)
            if (nearest != null && _nearestHexCache.Count < _maxCacheSize)
            {
                _nearestHexCache[hash] = nearest;
            }

            return nearest;
        }

        /// <summary>
        /// Проверить, безопасен ли путь к цели
        /// </summary>
        public bool IsPathSafe(Vector3 start, Vector3 target, IEnumerable<ICharacter> enemies, float safetyRadius = 5f)
        {
            if (enemies == null) return true;

            var pathDirection = (target - start).normalized;
            var pathLength = Vector3.Distance(start, target);

            foreach (var enemy in enemies)
            {
                if (enemy?.transform == null) continue;

                var enemyPosition = enemy.transform.position;
                var toEnemy = enemyPosition - start;
                var projectionLength = Vector3.Dot(toEnemy, pathDirection);
                
                // Враг находится на пути
                if (projectionLength > 0 && projectionLength < pathLength)
                {
                    var projection = start + pathDirection * projectionLength;
                    var distanceToPath = Vector3.Distance(enemyPosition, projection);
                    
                    if (distanceToPath < safetyRadius)
                        return false;
                }
            }
            
            return true;
        }

        /// <summary>
        /// Получить центр территории
        /// </summary>
        public IHex GetTerritoryCenter(IReadOnlyCollection<IHex> ownedHexes)
        {
            if (ownedHexes == null || ownedHexes.Count == 0) return null;

            var center = Vector3.zero;
            foreach (var hex in ownedHexes)
            {
                center += hex.transform.position;
            }
            center /= ownedHexes.Count;

            // Найти ближайший гекс к центру
            return FindNearestHex(center, ownedHexes);
        }

        /// <summary>
        /// Получить безопасную позицию для отступления
        /// </summary>
        public IHex GetSafeRetreatPosition(IReadOnlyCollection<IHex> ownedHexes, ICharacter nearestThreat)
        {
            if (ownedHexes == null || ownedHexes.Count == 0) return null;

            if (nearestThreat == null)
            {
                // Нет угрозы - возвращаемся к центру территории
                return GetTerritoryCenter(ownedHexes);
            }

            // Есть угроза - ищем самый дальний безопасный гекс
            var threatPosition = nearestThreat.transform.position;
            
            return ownedHexes
                .OrderByDescending(hex => Vector3.Distance(hex.transform.position, threatPosition))
                .FirstOrDefault();
        }

        /// <summary>
        /// Проверить, безопасна ли позиция для исследования
        /// </summary>
        public bool IsPositionSafeForExploration(Vector3 position, IEnumerable<ICharacter> nearbyEnemies, float safeDistance = 6f)
        {
            if (nearbyEnemies == null) return true;

            foreach (var enemy in nearbyEnemies)
            {
                if (enemy?.transform == null) continue;
                
                var distance = Vector3.Distance(position, enemy.transform.position);
                if (distance < safeDistance)
                    return false;
            }
            
            return true;
        }

        /// <summary>
        /// Вычислить расстояние от позиции до ближайшего гекса территории
        /// </summary>
        public float GetDistanceFromTerritory(Vector3 position, IReadOnlyCollection<IHex> ownedHexes)
        {
            if (ownedHexes == null || ownedHexes.Count == 0) return float.MaxValue;

            var nearestOwnHex = FindNearestHex(position, ownedHexes);
            
            return nearestOwnHex != null 
                ? Vector3.Distance(position, nearestOwnHex.transform.position)
                : float.MaxValue;
        }

        #region Private Methods

        private int GetPositionHash(Vector3 position, int collectionSize)
        {
            // Простой хэш для кэширования (округляем до сетки)
            var gridPos = new Vector3(
                Mathf.Round(position.x * 2f) / 2f,
                0f,
                Mathf.Round(position.z * 2f) / 2f
            );
            
            return (gridPos.GetHashCode() * 397) ^ collectionSize;
        }

        private void CleanupCache()
        {
            if (_nearestHexCache.Count > _maxCacheSize * 0.8f)
            {
                // Очищаем 20% кэша для WebGL
                var itemsToRemove = _nearestHexCache.Count / 5;
                var keysToRemove = _nearestHexCache.Keys.Take(itemsToRemove).ToList();
                
                foreach (var key in keysToRemove)
                {
                    _nearestHexCache.Remove(key);
                }

                Debug.Log($"[BehaviorUtilsService] Cache cleaned up, removed {itemsToRemove} items");
            }
        }

        #endregion

        #region Inspector Tools

        [ContextMenu("Clear Cache")]
        private void ClearCache()
        {
            _nearestHexCache.Clear();
            Debug.Log("[BehaviorUtilsService] Cache cleared manually");
        }

        [ContextMenu("Print Cache Stats")]
        private void PrintCacheStats()
        {
            Debug.Log($"[BehaviorUtilsService] Cache: {_nearestHexCache.Count}/{_maxCacheSize} items");
        }

        #endregion
    }
}