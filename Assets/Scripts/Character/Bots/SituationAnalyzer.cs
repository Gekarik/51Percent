using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Оптимизированный анализатор ситуации с кэшированием
/// </summary>
public class SituationAnalyzer
{
    private readonly IHexGridProvider _grid;
    private readonly BotStateConfig _config;
    
    // Кэш для производительности
    private IGrabbable[] _cachedResources;
    private ICharacter[] _cachedEnemies;
    private float _lastCacheUpdate;
    private const float CACHE_UPDATE_INTERVAL = 2f; // Обновляем кэш каждые 2 секунды
    private const int MAX_SEARCH_ITEMS = 20; // Максимум объектов для анализа
    
    public SituationAnalyzer(IHexGridProvider grid, BotStateConfig config)
    {
        _grid = grid;
        _config = config;
    }
    
    /// <summary>
    /// Обновляет кэш объектов если необходимо
    /// </summary>
    private void UpdateCacheIfNeeded()
    {
        if (Time.time - _lastCacheUpdate > CACHE_UPDATE_INTERVAL || _cachedResources == null || _cachedEnemies == null)
        {
            // Кэшируем ресурсы (только активные)
            var resources = Object.FindObjectsOfType<MonoBehaviour>()
                .OfType<IGrabbable>()
                .Where(g => g != null && g.State == GrabbableState.Idle)
                .Take(MAX_SEARCH_ITEMS)
                .ToArray();
            _cachedResources = resources ?? new IGrabbable[0];
                
            // Кэшируем врагов (только живых)
            var enemies = Object.FindObjectsOfType<MonoBehaviour>()
                .OfType<ICharacter>()
                .Where(c => c != null && c.State == CharacterState.Alive)
                .Take(MAX_SEARCH_ITEMS)
                .ToArray();
            _cachedEnemies = enemies ?? new ICharacter[0];
                
            _lastCacheUpdate = Time.time;
            
            // Debug информация (только для диагностики)
            // Debug.Log($"Cache updated: {_cachedResources.Length} resources, {_cachedEnemies.Length} enemies");
        }
    }

    /// <summary>
    /// Быстрый поиск ближайших ресурсов (с кэшированием)
    /// </summary>
    public List<IGrabbable> FindNearbyResources(Vector3 position, float range)
    {
        UpdateCacheIfNeeded();
        
        var resources = new List<IGrabbable>(5); // Предварительно выделяем память
        float rangeSqr = range * range; // Избегаем корень для ускорения
        
        for (int i = 0; i < _cachedResources.Length; i++)
        {
            var resource = _cachedResources[i];
            if (resource == null || resource.State != GrabbableState.Idle) continue;
            
            if (resource is MonoBehaviour mb)
            {
                float distanceSqr = (position - mb.transform.position).sqrMagnitude;
                if (distanceSqr <= rangeSqr)
                {
                    resources.Add(resource);
                    if (resources.Count >= 5) break; // Ограничиваем количество
                }
            }
        }
        
        // Простая сортировка только если нашли ресурсы
        if (resources.Count > 1)
        {
            resources.Sort((a, b) => 
            {
                var aMb = a as MonoBehaviour;
                var bMb = b as MonoBehaviour;
                if (aMb == null || bMb == null) return 0;
                
                float distA = (position - aMb.transform.position).sqrMagnitude;
                float distB = (position - bMb.transform.position).sqrMagnitude;
                return distA.CompareTo(distB);
            });
        }
        
        return resources;
    }
    
    /// <summary>
    /// Быстрый поиск вражеских трейлов (оптимизированный)
    /// </summary>
    public List<IHex> FindVulnerableEnemyTrails(ICharacter self, Vector3 position, float range)
    {
        var vulnerableTrails = new List<IHex>(5); // Предварительно выделяем память
        float rangeSqr = range * range;
        int foundCount = 0;
        
        // Ограничиваем поиск по сетке для производительности
        foreach (var hex in _grid.AllHexes)
        {
            // Early exit если уже нашли достаточно
            if (foundCount >= 5) break;
            
            // Быстрые проверки сначала
            if (hex.State != HexState.PartOfTrail || hex.Owner == self || hex.Owner == null) 
                continue;
                
            float distanceSqr = (position - hex.transform.position).sqrMagnitude;
            if (distanceSqr <= rangeSqr)
            {
                vulnerableTrails.Add(hex);
                foundCount++;
            }
        }
        
        // Сортируем только если нашли несколько
        if (vulnerableTrails.Count > 1)
        {
            vulnerableTrails.Sort((a, b) => 
            {
                float distA = (position - a.transform.position).sqrMagnitude;
                float distB = (position - b.transform.position).sqrMagnitude;
                return distA.CompareTo(distB);
            });
        }
        
        return vulnerableTrails;
    }
    
    /// <summary>
    /// Быстрая проверка угрозы от врагов (с кэшированием)
    /// </summary>
    public bool IsThreatenedByEnemy(ICharacter self, Vector3 position, float range)
    {
        UpdateCacheIfNeeded();
        
        float rangeSqr = range * range;
        
        for (int i = 0; i < _cachedEnemies.Length; i++)
        {
            var enemy = _cachedEnemies[i];
            if (enemy == null || enemy == self || enemy.State != CharacterState.Alive) continue;
            
            if (enemy is MonoBehaviour mb)
            {
                float distanceSqr = (position - mb.transform.position).sqrMagnitude;
                if (distanceSqr <= rangeSqr)
                {
                    return true; // Найдена угроза - выходим немедленно
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Быстрый поиск безопасных гексов (упрощенный для производительности)
    /// </summary>
    public List<IHex> FindSafeExpansionTargets(ICharacter self, Vector3 position, int maxDistance)
    {
        UpdateCacheIfNeeded();
        
        var safeTargets = new List<IHex>(10);
        float maxDistanceSqr = (maxDistance * _grid.CellDiameter) * (maxDistance * _grid.CellDiameter);
        int foundCount = 0;
        
        // Ограниченный поиск для производительности
        foreach (var hex in _grid.AllHexes)
        {
            if (foundCount >= 10) break; // Максимум 10 целей
            
            // Быстрые проверки
            if (hex.State != HexState.Empty) continue;
            
            float distanceSqr = (position - hex.transform.position).sqrMagnitude;
            if (distanceSqr > maxDistanceSqr) continue;
            
            // Упрощенная проверка безопасности - используем кэш врагов
            bool isSafe = true;
            for (int i = 0; i < _cachedEnemies.Length && i < 5; i++) // Проверяем только первых 5 врагов
            {
                var enemy = _cachedEnemies[i];
                if (enemy == null || enemy == self) continue;
                
                if (enemy is MonoBehaviour mb)
                {
                    float enemyDistSqr = (hex.transform.position - mb.transform.position).sqrMagnitude;
                    if (enemyDistSqr < _config.detectionRange * _config.detectionRange)
                    {
                        isSafe = false;
                        break;
                    }
                }
            }
            
            if (isSafe)
            {
                safeTargets.Add(hex);
                foundCount++;
            }
        }
        
        // Простая сортировка только если нашли несколько
        if (safeTargets.Count > 1)
        {
            safeTargets.Sort((a, b) => 
            {
                float distA = (position - a.transform.position).sqrMagnitude;
                float distB = (position - b.transform.position).sqrMagnitude;
                return distA.CompareTo(distB);
            });
        }
        
        return safeTargets;
    }
    
    /// <summary>
    /// Оценивает приоритет состояния на основе текущей ситуации
    /// </summary>
    public int EvaluateStatePriority(BotState state, ICharacter self, Vector3 position)
    {
        int priority = 0;
        
        switch (state)
        {
            case BotState.Escape:
                // Высший приоритет если есть угроза
                if (IsThreatenedByEnemy(self, position, _config.detectionRange))
                    priority = 100;
                break;
                
            case BotState.Attack:
                // Приоритет зависит от количества доступных целей
                var trails = FindVulnerableEnemyTrails(self, position, _config.detectionRange);
                priority = Mathf.Min(trails.Count * 20, 80);
                break;
                
            case BotState.Collect:
                // Приоритет зависит от количества ресурсов поблизости
                var resources = FindNearbyResources(position, _config.detectionRange);
                priority = Mathf.Min(resources.Count * 15, 70);
                break;
                
            case BotState.Expand:
                // Базовый приоритет для расширения
                var safeTargets = FindSafeExpansionTargets(self, position, (int)_config.detectionRange);
                priority = safeTargets.Count > 0 ? 60 : 20;
                break;
                
            case BotState.Return:
                // Приоритет зависит от того, есть ли активный трейл
                var conquester = ((MonoBehaviour)self).GetComponent<Conquester>();
                // Проверяем, есть ли активный трейл (это требует доступа к приватным полям, упростим)
                priority = 40;
                break;
        }
        
        return priority;
    }
}