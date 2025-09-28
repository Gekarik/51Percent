using System;
using System.Collections;
using UnityEngine;
using AI.Core;

namespace AI.Components
{
    /// <summary>
    /// Управляет жизненным циклом AI - корутины, обновления, состояния
    /// Выделен из AIBrain для разделения ответственности
    /// </summary>
    public class AILifecycleManager : MonoBehaviour
    {
        [Header("Lifecycle Settings")]
        [SerializeField, Range(0.1f, 1f)] private float _thinkInterval = 0.2f;
        [SerializeField] private bool _enableLifecycleLogging = false;

        private Coroutine _thinkingCoroutine;
        private AIState _currentState = AIState.Initializing;
        private bool _isInitialized = false;

        // События для уведомления других компонентов
        public event Action<AIState> OnStateChanged;
        public event Action OnThinkingCycle;

        public AIState CurrentState => _currentState;
        public bool IsInitialized => _isInitialized;
        public float ThinkInterval => _thinkInterval;

        /// <summary>
        /// Инициализировать менеджер жизненного цикла
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
            {
                Debug.LogWarning($"[AILifecycleManager] {name} already initialized");
                return;
            }

            _isInitialized = true;
            ChangeState(AIState.Thinking);

            if (_enableLifecycleLogging)
                Debug.Log($"[AILifecycleManager] {name} initialized");
        }

        /// <summary>
        /// Запустить цикл мышления
        /// </summary>
        public void StartThinking()
        {
            if (!_isInitialized)
            {
                Debug.LogWarning($"[AILifecycleManager] Cannot start thinking - {name} not initialized");
                return;
            }

            if (_thinkingCoroutine != null)
            {
                Debug.LogWarning($"[AILifecycleManager] {name} already thinking");
                return;
            }

            _thinkingCoroutine = StartCoroutine(ThinkingLoop());

            if (_enableLifecycleLogging)
                Debug.Log($"[AILifecycleManager] {name} started thinking");
        }

        /// <summary>
        /// Остановить цикл мышления
        /// </summary>
        public void StopThinking()
        {
            if (_thinkingCoroutine != null)
            {
                StopCoroutine(_thinkingCoroutine);
                _thinkingCoroutine = null;

                if (_enableLifecycleLogging)
                    Debug.Log($"[AILifecycleManager] {name} stopped thinking");
            }

            ChangeState(AIState.Dead);
        }

        /// <summary>
        /// Изменить состояние AI
        /// </summary>
        public void ChangeState(AIState newState)
        {
            if (_currentState == newState) return;

            var oldState = _currentState;
            _currentState = newState;

            if (_enableLifecycleLogging)
                Debug.Log($"[AILifecycleManager] {name} state changed: {oldState} -> {newState}");

            OnStateChanged?.Invoke(newState);
        }

        /// <summary>
        /// Установить интервал мышления
        /// </summary>
        public void SetThinkInterval(float interval)
        {
            _thinkInterval = Mathf.Clamp(interval, 0.1f, 1f);

            if (_enableLifecycleLogging)
                Debug.Log($"[AILifecycleManager] {name} think interval set to {_thinkInterval:F2}");
        }

        /// <summary>
        /// Временно приостановить мышление
        /// </summary>
        public void PauseThinking()
        {
            if (_currentState == AIState.Thinking)
            {
                ChangeState(AIState.Waiting);

                if (_enableLifecycleLogging)
                    Debug.Log($"[AILifecycleManager] {name} thinking paused");
            }
        }

        /// <summary>
        /// Возобновить мышление
        /// </summary>
        public void ResumeThinking()
        {
            if (_currentState == AIState.Waiting)
            {
                ChangeState(AIState.Thinking);

                if (_enableLifecycleLogging)
                    Debug.Log($"[AILifecycleManager] {name} thinking resumed");
            }
        }

        #region Unity Lifecycle

        private void OnEnable()
        {
            if (_isInitialized && _thinkingCoroutine == null)
            {
                StartThinking();
            }
        }

        private void OnDisable()
        {
            StopThinking();
        }

        private void OnDestroy()
        {
            StopThinking();
            OnStateChanged = null;
            OnThinkingCycle = null;
        }

        #endregion

        #region Private Methods

        private IEnumerator ThinkingLoop()
        {
            while (_isInitialized)
            {
                // Пропускаем мышление если не в нужном состоянии
                if (_currentState == AIState.Thinking)
                {
                    try
                    {
                        ChangeState(AIState.Acting);
                        
                        // Уведомляем о цикле мышления
                        OnThinkingCycle?.Invoke();
                        
                        // Возвращаемся к мышлению
                        if (_currentState == AIState.Acting)
                            ChangeState(AIState.Thinking);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[AILifecycleManager] Error in thinking cycle for {name}: {ex}");
                        ChangeState(AIState.Dead);
                        break;
                    }
                }

                yield return new WaitForSeconds(_thinkInterval);
            }

            if (_enableLifecycleLogging)
                Debug.Log($"[AILifecycleManager] {name} thinking loop ended");
        }

        #endregion

        #region Inspector Tools

        [ContextMenu("Force Start Thinking")]
        private void ForceStartThinking()
        {
            if (!_isInitialized)
                Initialize();
            StartThinking();
        }

        [ContextMenu("Force Stop Thinking")]
        private void ForceStopThinking()
        {
            StopThinking();
        }

        [ContextMenu("Pause/Resume Thinking")]
        private void ToggleThinking()
        {
            if (_currentState == AIState.Thinking)
                PauseThinking();
            else if (_currentState == AIState.Waiting)
                ResumeThinking();
        }

        [ContextMenu("Print State Info")]
        private void PrintStateInfo()
        {
            Debug.Log($"[AILifecycleManager] {name} - State: {_currentState}, Initialized: {_isInitialized}, Thinking: {_thinkingCoroutine != null}");
        }

        #endregion
    }
}