using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Провайдер направления движения для IoGameBot
/// </summary>
public class IoGameBotPathProvider : VectorProviderComponent
{
    private List<Vector3> _currentPath = new List<Vector3>();
    private int _currentPathIndex = 0;
    private float _waypointReachDistance = 0.3f;
    
    public bool IsPathCompleted => _currentPathIndex >= _currentPath.Count;
    public int CurrentWaypointIndex => _currentPathIndex;
    public int TotalWaypoints => _currentPath.Count;
    
    /// <summary>
    /// Устанавливает новый путь для бота
    /// </summary>
    public void SetPath(List<Vector3> path)
    {
        _currentPath.Clear();
        if (path != null)
        {
            _currentPath.AddRange(path);
        }
        _currentPathIndex = 0;
        
        Debug.Log($"Bot {gameObject.name}: Set new path with {_currentPath.Count} waypoints");
    }
    
    /// <summary>
    /// Очищает текущий путь
    /// </summary>
    public void ClearPath()
    {
        _currentPath.Clear();
        _currentPathIndex = 0;
    }
    
    public override Vector3 GetMoveDirection()
    {
        if (_currentPath.Count == 0 || _currentPathIndex >= _currentPath.Count)
        {
            return Vector3.zero;
        }
        
        Vector3 targetPosition = _currentPath[_currentPathIndex];
        Vector3 direction = (targetPosition - transform.position);
        direction.y = 0; // Игнорируем высоту
        
        // Проверяем, достигли ли waypoint
        if (direction.magnitude <= _waypointReachDistance)
        {
            _currentPathIndex++;
            
            // Если достигли последней точки
            if (_currentPathIndex >= _currentPath.Count)
            {
                Debug.Log($"Bot {gameObject.name}: Completed path");
                return Vector3.zero;
            }
            
            // Переходим к следующей точке
            targetPosition = _currentPath[_currentPathIndex];
            direction = (targetPosition - transform.position);
            direction.y = 0;
        }
        
        return direction.normalized;
    }
}