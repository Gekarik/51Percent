using UnityEngine;

/// <summary>
/// Интерфейс для получения границ игровой карты
/// </summary>
public interface IMapBounds
{
    /// <summary>
    /// Границы игрового поля
    /// </summary>
    Bounds PlayableArea { get; }
    
    /// <summary>
    /// Проверяет, находится ли точка в пределах игрового поля
    /// </summary>
    bool IsInBounds(Vector3 position);
    
    /// <summary>
    /// Ограничивает позицию границами карты
    /// </summary>
    Vector3 ClampToBounds(Vector3 position);
}