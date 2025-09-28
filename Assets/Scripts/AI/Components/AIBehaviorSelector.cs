using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AI.Behaviors;
using AI.Core;

namespace AI.Components
{
    /// <summary>
    /// Компонент для выбора лучшего поведения AI
    /// Выделен из AIBrain для упрощения логики
    /// </summary>
    public class AIBehaviorSelector : MonoBehaviour
    {
        [Header("Selection Settings")]
        [SerializeField] private bool _enableSelectionLogging = false;
        
        private List<IAIBehavior> _availableBehaviors;
        private IAIBehavior _currentBehavior;

        public IAIBehavior CurrentBehavior => _currentBehavior;
        public IReadOnlyList<IAIBehavior> AvailableBehaviors => _availableBehaviors?.AsReadOnly();

        /// <summary>
        /// Инициализировать доступные поведения
        /// </summary>
        public void InitializeBehaviors(PathProvider pathProvider, BotPersonality personality)
        {
            _availableBehaviors = new List<IAIBehavior>
            {
                new IdleBehavior(pathProvider),
                new ExploreBehavior(pathProvider)
                // TODO: Добавить другие поведения когда будут готовы
                // new AttackBehavior(pathProvider),
                // new DefendBehavior(pathProvider),
                // new EscapeBehavior(pathProvider)
            };

            if (_enableSelectionLogging)
                Debug.Log($"[AIBehaviorSelector] Initialized {_availableBehaviors.Count} behaviors for {name}");
        }

        /// <summary>
        /// Выбрать лучшее поведение для текущей ситуации
        /// </summary>
        public IAIBehavior SelectBestBehavior(AIContext context)
        {
            if (context == null || _availableBehaviors == null || _availableBehaviors.Count == 0)
                return null;

            IAIBehavior bestBehavior = null;
            var highestPriority = BehaviorPriority.VeryLow;

            // Проверяем каждое поведение
            foreach (var behavior in _availableBehaviors)
            {
                try
                {
                    if (behavior.CanExecute(context))
                    {
                        var priority = behavior.GetPriority(context);
                        
                        if (priority > highestPriority)
                        {
                            highestPriority = priority;
                            bestBehavior = behavior;
                        }
                        
                        if (_enableSelectionLogging)
                        {
                            Debug.Log($"[AIBehaviorSelector] {behavior.Name} priority: {priority} (can execute: {behavior.CanExecute(context)})");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[AIBehaviorSelector] Error checking behavior {behavior.Name}: {ex}");
                }
            }

            // Если ничего не найдено, возвращаем IdleBehavior как fallback
            if (bestBehavior == null)
            {
                bestBehavior = _availableBehaviors.FirstOrDefault(b => b is IdleBehavior);
                
                if (_enableSelectionLogging)
                    Debug.Log("[AIBehaviorSelector] No suitable behavior found, falling back to Idle");
            }

            return bestBehavior;
        }

        /// <summary>
        /// Безопасно переключить поведение
        /// </summary>
        public bool SwitchBehavior(IAIBehavior newBehavior, AIContext context)
        {
            if (newBehavior == _currentBehavior) 
                return true; // Уже используем это поведение

            try
            {
                // Выходим из старого поведения
                if (_currentBehavior != null)
                {
                    _currentBehavior.OnExit(context);
                    
                    if (_enableSelectionLogging)
                        Debug.Log($"[AIBehaviorSelector] {name} exiting behavior: {_currentBehavior.Name}");
                }

                // Входим в новое поведение
                _currentBehavior = newBehavior;
                
                if (_currentBehavior != null)
                {
                    _currentBehavior.OnEnter(context);
                    
                    if (_enableSelectionLogging)
                        Debug.Log($"[AIBehaviorSelector] {name} entering behavior: {_currentBehavior.Name}");
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AIBehaviorSelector] Error switching behavior for {name}: {ex}");
                _currentBehavior = null;
                return false;
            }
        }

        /// <summary>
        /// Принудительно установить поведение (для тестирования/отладки)
        /// </summary>
        public void ForceBehavior(IAIBehavior behavior, AIContext context)
        {
            if (_availableBehaviors == null || !_availableBehaviors.Contains(behavior))
            {
                Debug.LogWarning($"[AIBehaviorSelector] Trying to force unknown behavior: {behavior?.Name}");
                return;
            }

            SwitchBehavior(behavior, context);
        }

        /// <summary>
        /// Остановить текущее поведение
        /// </summary>
        public void StopCurrentBehavior(AIContext context)
        {
            if (_currentBehavior != null)
            {
                _currentBehavior.OnExit(context);
                _currentBehavior = null;
                
                if (_enableSelectionLogging)
                    Debug.Log($"[AIBehaviorSelector] {name} stopped current behavior");
            }
        }

        /// <summary>
        /// Получить поведение по типу
        /// </summary>
        public T GetBehavior<T>() where T : class, IAIBehavior
        {
            return _availableBehaviors?.FirstOrDefault(b => b is T) as T;
        }

        #region Inspector Tools

        [ContextMenu("List Available Behaviors")]
        private void ListAvailableBehaviors()
        {
            if (_availableBehaviors == null)
            {
                Debug.Log("[AIBehaviorSelector] No behaviors initialized");
                return;
            }

            var behaviorNames = _availableBehaviors.Select(b => b.Name).ToArray();
            Debug.Log($"[AIBehaviorSelector] Available behaviors: {string.Join(", ", behaviorNames)}");
        }

        [ContextMenu("Show Current Behavior")]
        private void ShowCurrentBehavior()
        {
            Debug.Log($"[AIBehaviorSelector] Current behavior: {_currentBehavior?.Name ?? "None"}");
        }

        #endregion
    }
}