using System;
using System.Collections.Generic;
using UnityEngine;

namespace AI.Services
{
    /// <summary>
    /// Сервис для pathfinding - заменяет статический Pathfinder
    /// Оптимизирован для WebGL с управлением памятью
    /// </summary>
    public class PathfindingService : MonoBehaviour
    {
        [Header("Performance Settings")]
        [SerializeField] private int _maxCacheSize = 100;
        [SerializeField] private float _cacheCleanupInterval = 60f;
        [SerializeField] private int _maxPathLength = 50;

        // Кэш путей для оптимизации (с ограничением для WebGL)
        private Dictionary<PathKey, List<IHex>> _pathCache;
        private float _lastCleanupTime;

        private void Awake()
        {
            _pathCache = new Dictionary<PathKey, List<IHex>>();
            _lastCleanupTime = Time.time;
        }

        private void Update()
        {
            // Периодическая очистка кэша
            if (Time.time - _lastCleanupTime > _cacheCleanupInterval)
            {
                CleanupCache();
                _lastCleanupTime = Time.time;
            }
        }

        /// <summary>
        /// Найти путь от начальной до конечной точки (A* алгоритм)
        /// </summary>
        public List<IHex> FindPath(IHex start, IHex goal, IHexGridProvider grid, Func<IHex, bool> canEnter)
        {
            if (start == null || goal == null || grid == null || canEnter == null)
                return new List<IHex>();

            // Проверяем кэш
            var pathKey = new PathKey(start, goal);
            if (_pathCache.TryGetValue(pathKey, out var cachedPath) && IsPathValid(cachedPath, canEnter))
            {
                return new List<IHex>(cachedPath); // Возвращаем копию
            }

            // Вычисляем новый путь
            var path = AStar(start, goal, grid, canEnter);
            
            // Кэшируем если путь не слишком длинный и кэш не переполнен
            if (path.Count > 0 && path.Count <= _maxPathLength && _pathCache.Count < _maxCacheSize)
            {
                _pathCache[pathKey] = new List<IHex>(path);
            }

            return path;
        }

        /// <summary>
        /// Проверить, валиден ли путь (все гексы доступны)
        /// </summary>
        public bool IsPathValid(List<IHex> path, Func<IHex, bool> canEnter)
        {
            if (path == null || path.Count == 0) return false;

            foreach (var hex in path)
            {
                if (hex == null || !canEnter(hex))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Найти безопасный путь избегая врагов
        /// </summary>
        public List<IHex> FindSafePath(IHex start, IHex goal, IHexGridProvider grid, 
            IEnumerable<ICharacter> enemies, float safetyRadius = 3f)
        {
            return FindPath(start, goal, grid, hex =>
            {
                // Базовая проверка
                if (hex.Owner != null && hex.State == HexState.Busy && hex.Owner != start.Owner)
                    return false;

                // Проверка безопасности от врагов
                if (enemies != null)
                {
                    foreach (var enemy in enemies)
                    {
                        if (enemy?.transform == null) continue;
                        
                        var distance = Vector3.Distance(hex.transform.position, enemy.transform.position);
                        if (distance < safetyRadius)
                            return false;
                    }
                }

                return true;
            });
        }

        /// <summary>
        /// Очистить кэш путей
        /// </summary>
        public void ClearPathCache()
        {
            _pathCache.Clear();
            Debug.Log("[PathfindingService] Path cache cleared");
        }

        #region Private A* Implementation

        private List<IHex> AStar(IHex start, IHex goal, IHexGridProvider grid, Func<IHex, bool> canEnter)
        {
            var openSet = new SimplePriorityQueue<IHex>();
            openSet.Enqueue(start, 0);

            var cameFrom = new Dictionary<IHex, IHex>();
            var gScore = new Dictionary<IHex, float> { { start, 0 } };
            var fScore = new Dictionary<IHex, float> { { start, Heuristic(start, goal) } };

            var iterationCount = 0;
            var maxIterations = _maxPathLength * 10; // Предотвращаем бесконечные циклы

            while (openSet.Count > 0 && iterationCount < maxIterations)
            {
                iterationCount++;
                var current = openSet.Dequeue();
                
                if (current == goal) 
                    return ReconstructPath(cameFrom, current);

                foreach (var neighbor in grid.GetNeighbors(current))
                {
                    if (!canEnter(neighbor)) continue;
                    
                    float tentativeG = gScore[current] + 1f;
                    
                    if (!gScore.TryGetValue(neighbor, out var oldG) || tentativeG < oldG)
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeG;
                        float f = tentativeG + Heuristic(neighbor, goal);
                        fScore[neighbor] = f;
                        openSet.EnqueueOrUpdate(neighbor, f);
                    }
                }
            }

            if (iterationCount >= maxIterations)
            {
                Debug.LogWarning("[PathfindingService] A* reached max iterations, path may be incomplete");
            }

            return new List<IHex>(); // Путь не найден
        }

        private float Heuristic(IHex a, IHex b)
        {
            return Vector3.Distance(a.transform.position, b.transform.position);
        }

        private List<IHex> ReconstructPath(Dictionary<IHex, IHex> cameFrom, IHex current)
        {
            var path = new List<IHex> { current };
            
            while (cameFrom.TryGetValue(current, out var prev))
            {
                current = prev;
                path.Add(current);
            }
            
            path.Reverse();
            return path;
        }

        #endregion

        #region Cache Management

        private void CleanupCache()
        {
            if (_pathCache.Count > _maxCacheSize * 0.8f)
            {
                // Удаляем 25% старых записей
                var itemsToRemove = _pathCache.Count / 4;
                var keysToRemove = new List<PathKey>();
                
                foreach (var key in _pathCache.Keys)
                {
                    keysToRemove.Add(key);
                    if (keysToRemove.Count >= itemsToRemove) break;
                }
                
                foreach (var key in keysToRemove)
                {
                    _pathCache.Remove(key);
                }

                Debug.Log($"[PathfindingService] Cache cleaned up, removed {itemsToRemove} paths");
            }
        }

        #endregion

        #region Helper Classes

        private struct PathKey : IEquatable<PathKey>
        {
            public readonly int StartId;
            public readonly int GoalId;

            public PathKey(IHex start, IHex goal)
            {
                StartId = start?.GetInstanceID() ?? 0;
                GoalId = goal?.GetInstanceID() ?? 0;
            }

            public bool Equals(PathKey other)
            {
                return StartId == other.StartId && GoalId == other.GoalId;
            }

            public override int GetHashCode()
            {
                return (StartId * 397) ^ GoalId;
            }
        }

        #endregion

        #region Inspector Tools

        [ContextMenu("Clear Path Cache")]
        private void InspectorClearCache()
        {
            ClearPathCache();
        }

        [ContextMenu("Print Cache Stats")]
        private void PrintCacheStats()
        {
            Debug.Log($"[PathfindingService] Cache: {_pathCache.Count}/{_maxCacheSize} paths");
        }

        #endregion
    }

    /// <summary>
    /// Упрощённая приоритетная очередь для A* (оптимизированная для WebGL)
    /// </summary>
    internal class SimplePriorityQueue<T>
    {
        private readonly List<(T item, float priority)> _data = new List<(T, float)>();
        
        public int Count => _data.Count;
        
        public void Enqueue(T item, float priority)
        {
            _data.Add((item, priority));
        }
        
        public void EnqueueOrUpdate(T item, float priority)
        {
            for (int i = 0; i < _data.Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(_data[i].item, item))
                {
                    _data[i] = (item, priority);
                    return;
                }
            }
            Enqueue(item, priority);
        }
        
        public T Dequeue()
        {
            if (_data.Count == 0) return default(T);
            
            // Находим элемент с минимальным приоритетом
            var minIndex = 0;
            var minPriority = _data[0].priority;
            
            for (int i = 1; i < _data.Count; i++)
            {
                if (_data[i].priority < minPriority)
                {
                    minIndex = i;
                    minPriority = _data[i].priority;
                }
            }
            
            var best = _data[minIndex];
            _data.RemoveAt(minIndex);
            return best.item;
        }
    }
}