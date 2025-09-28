using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Улучшенный PathProvider для адаптивного движения бота
/// </summary>
public class AdaptivePathProvider : VectorProviderComponent
{
    private readonly List<Vector3> _waypoints = new List<Vector3>();
    private int _currentWaypoint = 0;
    private float _waypointReachDistance = 0.5f;
    
    // Для движения к ресурсам
    private IGrabbable _targetResource;
    private IHex _targetHex;
    
    public bool IsDone => _currentWaypoint >= _waypoints.Count && _targetResource == null && _targetHex == null;
    
    /// <summary>
    /// Устанавливает путь из гексов
    /// </summary>
    public void SetPath(List<IHex> hexPath)
    {
        _waypoints.Clear();
        _targetResource = null;
        _targetHex = null;
        
        if (hexPath != null && hexPath.Count > 0)
        {
            foreach (var hex in hexPath)
            {
                _waypoints.Add(hex.transform.position);
            }
        }
        
        _currentWaypoint = 0;
    }
    
    /// <summary>
    /// Устанавливает цель-ресурс для сбора
    /// </summary>
    public void SetResourceTarget(IGrabbable resource)
    {
        _waypoints.Clear();
        _targetResource = resource;
        _targetHex = null;
        _currentWaypoint = 0;
    }
    
    /// <summary>
    /// Устанавливает цель-гекс для атаки/захвата
    /// </summary>
    public void SetHexTarget(IHex hex)
    {
        _waypoints.Clear();
        _targetResource = null;
        _targetHex = hex;
        _currentWaypoint = 0;
    }
    
    /// <summary>
    /// Останавливает движение
    /// </summary>
    public void Stop()
    {
        _waypoints.Clear();
        _targetResource = null;
        _targetHex = null;
        _currentWaypoint = 0;
    }
    
    public override Vector3 GetMoveDirection()
    {
        Vector3 targetPosition = Vector3.zero;
        bool hasTarget = false;
        
        // Приоритет: ресурс > конкретный гекс > путь из точек
        if (_targetResource != null && _targetResource is MonoBehaviour resourceMb)
        {
            // Проверяем, что ресурс еще доступен
            if (_targetResource.State == GrabbableState.Idle)
            {
                targetPosition = resourceMb.transform.position;
                hasTarget = true;
            }
            else
            {
                _targetResource = null; // Ресурс собран, сбрасываем цель
            }
        }
        else if (_targetHex != null)
        {
            targetPosition = _targetHex.transform.position;
            hasTarget = true;
        }
        else if (_currentWaypoint < _waypoints.Count)
        {
            targetPosition = _waypoints[_currentWaypoint];
            hasTarget = true;
        }
        
        if (!hasTarget)
        {
            return Vector3.zero;
        }
        
        // Вычисляем направление к цели
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0; // Игнорируем высоту
        
        // Проверяем, достигли ли цели
        if (direction.magnitude < _waypointReachDistance)
        {
            if (_targetResource != null || _targetHex != null)
            {
                // Достигли целевого ресурса/гекса
                _targetResource = null;
                _targetHex = null;
            }
            else if (_currentWaypoint < _waypoints.Count)
            {
                // Переходим к следующей точке маршрута
                _currentWaypoint++;
            }
            
            return Vector3.zero;
        }
        
        return direction.normalized;
    }
    
    /// <summary>
    /// Получает текущую цель движения для отладки
    /// </summary>
    public Vector3 GetCurrentTarget()
    {
        if (_targetResource != null && _targetResource is MonoBehaviour resourceMb)
        {
            return resourceMb.transform.position;
        }
        else if (_targetHex != null)
        {
            return _targetHex.transform.position;
        }
        else if (_currentWaypoint < _waypoints.Count)
        {
            return _waypoints[_currentWaypoint];
        }
        
        return transform.position;
    }
}