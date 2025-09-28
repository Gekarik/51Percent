using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Анализирует текущую ситуацию на карте для принятия решений ботом
/// </summary>
public class SituationAnalyzer
{
    private readonly IHexGridProvider _grid;
    private readonly BotStateConfig _config;
    
    public SituationAnalyzer(IHexGridProvider grid, BotStateConfig config)
    {
        _grid = grid;
        _config = config;
    }
    
    /// <summary>
    /// Находит ближайшие ресурсы (монетки/бустеры)
    /// </summary>
    public List<IGrabbable> FindNearbyResources(Vector3 position, float range)
    {
        var resources = new List<IGrabbable>();
        
        // Поиск всех grabbable объектов в сцене
        var allGrabbables = Object.FindObjectsOfType<MonoBehaviour>()
            .OfType<IGrabbable>()
            .Where(g => g.State == GrabbableState.Idle);
            
        foreach (var resource in allGrabbables)
        {
            if (resource is MonoBehaviour mb)
            {
                float distance = Vector3.Distance(position, mb.transform.position);
                if (distance <= range)
                {
                    resources.Add(resource);
                }
            }
        }
        
        return resources.OrderBy(r => Vector3.Distance(position, ((MonoBehaviour)r).transform.position)).ToList();
    }
    
    /// <summary>
    /// Находит уязвимые вражеские трейлы для атаки
    /// </summary>
    public List<IHex> FindVulnerableEnemyTrails(ICharacter self, Vector3 position, float range)
    {
        var vulnerableTrails = new List<IHex>();
        
        foreach (var hex in _grid.AllHexes)
        {
            // Ищем гексы, которые являются частью чужого трейла
            if (hex.State == HexState.PartOfTrail && hex.Owner != self && hex.Owner != null)
            {
                float distance = Vector3.Distance(position, hex.transform.position);
                if (distance <= range)
                {
                    vulnerableTrails.Add(hex);
                }
            }
        }
        
        return vulnerableTrails.OrderBy(h => Vector3.Distance(position, h.transform.position)).ToList();
    }
    
    /// <summary>
    /// Определяет, есть ли угроза рядом с текущим трейлом
    /// </summary>
    public bool IsThreatenedByEnemy(ICharacter self, Vector3 position, float range)
    {
        // Находим всех врагов поблизости
        var enemies = Object.FindObjectsOfType<MonoBehaviour>()
            .OfType<ICharacter>()
            .Where(c => c != self && c.State == CharacterState.Alive);
            
        foreach (var enemy in enemies)
        {
            if (enemy is MonoBehaviour mb)
            {
                float distance = Vector3.Distance(position, mb.transform.position);
                if (distance <= range)
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Находит безопасные гексы для расширения территории
    /// </summary>
    public List<IHex> FindSafeExpansionTargets(ICharacter self, Vector3 position, int maxDistance)
    {
        var safeTargets = new List<IHex>();
        var ownedHexes = _grid.AllHexes.Where(h => h.Owner == self && h.State == HexState.Busy);
        
        foreach (var hex in _grid.AllHexes)
        {
            // Проверяем только свободные гексы
            if (hex.State != HexState.Empty)
                continue;
                
            float distance = Vector3.Distance(position, hex.transform.position);
            
            // Проверяем расстояние
            if (distance > maxDistance * _grid.CellDiameter)
                continue;
                
            // Проверяем, что рядом нет врагов
            bool isSafe = true;
            foreach (var enemy in Object.FindObjectsOfType<MonoBehaviour>().OfType<ICharacter>())
            {
                if (enemy != self && enemy.State == CharacterState.Alive && enemy is MonoBehaviour mb)
                {
                    float enemyDistance = Vector3.Distance(hex.transform.position, mb.transform.position);
                    if (enemyDistance < _config.detectionRange)
                    {
                        isSafe = false;
                        break;
                    }
                }
            }
            
            if (isSafe)
            {
                safeTargets.Add(hex);
            }
        }
        
        return safeTargets.OrderBy(h => Vector3.Distance(position, h.transform.position)).ToList();
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