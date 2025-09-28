using AI.Core;

namespace AI.Behaviors
{
    /// <summary>
    /// Интерфейс для поведений ИИ
    /// </summary>
    public interface IAIBehavior
    {
        /// <summary>
        /// Имя поведения для отладки
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Приоритет поведения
        /// </summary>
        Priority Priority { get; }

        /// <summary>
        /// Может ли поведение быть выполнено в данный момент
        /// </summary>
        bool CanExecute(AIContext context);

        /// <summary>
        /// Вызывается при начале выполнения поведения
        /// </summary>
        void OnEnter(AIContext context);

        /// <summary>
        /// Основная логика поведения
        /// </summary>
        BehaviorResult Execute(AIContext context);

        /// <summary>
        /// Вызывается при завершении поведения
        /// </summary>
        void OnExit(AIContext context);

        /// <summary>
        /// Время последнего обновления
        /// </summary>
        float LastUpdateTime { get; set; }
    }
}