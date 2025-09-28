public enum BotState
{
    /// <summary>
    /// Бот ожидает, анализирует ситуацию
    /// </summary>
    Idle,
    
    /// <summary>
    /// Расширяет собственную территорию безопасными трейлами
    /// </summary>
    Expand,
    
    /// <summary>
    /// Собирает монетки и бустеры
    /// </summary>
    Collect,
    
    /// <summary>
    /// Атакует чужие трейлы для их прерывания
    /// </summary>
    Attack,
    
    /// <summary>
    /// Убегает от опасности (чужой персонаж рядом с трейлом)
    /// </summary>
    Escape,
    
    /// <summary>
    /// Возвращается на безопасную территорию
    /// </summary>
    Return
}