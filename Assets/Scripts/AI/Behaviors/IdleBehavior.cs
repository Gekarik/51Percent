using AI.Core;
using UnityEngine;

namespace AI.Behaviors
{
    /// <summary>
    /// Поведение ожидания - базовое состояние, когда бот не знает что делать
    /// </summary>
    public class IdleBehavior : BaseBehavior
    {
        public override string Name => "Idle";
        public override Priority Priority => Priority.Low;

        private float _idleDuration;
        private float _maxIdleTime = 3f;

        public override bool CanExecute(AIContext context)
        {
            // Idle всегда доступен как fallback
            return base.CanExecute(context);
        }

        public override void OnEnter(AIContext context)
        {
            base.OnEnter(context);
            
            // Случайное время ожидания, чтобы боты не были синхронными
            _idleDuration = Random.Range(1f, _maxIdleTime);
            _maxIdleTime = context.Personality switch
            {
                BotPersonality.Aggressive => 1f,      // Агрессивные боты не ждут долго
                BotPersonality.Defensive => 4f,       // Оборонительные могут ждать дольше
                BotPersonality.Opportunist => 2f,     // Оппортунисты средне
                BotPersonality.Territorial => 3f,     // Территориальные терпеливы
                BotPersonality.Balanced => 2.5f,      // Сбалансированные
                _ => 2f
            };
            
            UpdateBehaviorStatus(context, "Waiting...");
        }

        public override BehaviorResult Execute(AIContext context)
        {
            LastUpdateTime = Time.time;
            
            // Проверяем экстренные ситуации - если в опасности, немедленно выходим
            if (context.IsInDanger())
            {
                UpdateBehaviorStatus(context, "Danger detected!");
                return BehaviorResult.Failure; // Позволяет другим поведениям взять управление
            }

            // Проверяем возможности для атаки
            if (context.HasAttackOpportunity() && context.Personality == BotPersonality.Aggressive)
            {
                UpdateBehaviorStatus(context, "Attack opportunity!");
                return BehaviorResult.Failure; // Даём AttackBehavior шанс
            }

            // Ждём указанное время
            if (GetExecutionTime() >= _idleDuration)
            {
                UpdateBehaviorStatus(context, "Idle time expired");
                return BehaviorResult.Success;
            }

            // Обновляем статус с таймером
            var remainingTime = _idleDuration - GetExecutionTime();
            UpdateBehaviorStatus(context, $"Waiting... ({remainingTime:F1}s left)");
            
            return BehaviorResult.Running;
        }

        public override void OnExit(AIContext context)
        {
            base.OnExit(context);
            
            // Записываем в Blackboard, что мы завершили ожидание
            context.Blackboard.Set("last_idle_time", Time.time);
        }
    }
}