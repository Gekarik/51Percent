using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AI.Behaviors;

namespace AI.Core
{
    /// <summary>
    /// Главный контроллер ИИ - управляет поведениями и принимает решения
    /// </summary>
    [RequireComponent(typeof(ICharacter), typeof(PathProvider))]
    public class AIBrain : MonoBehaviour
    {
        [Header("AI Settings")]
        [SerializeField] private BotPersonality _personality = BotPersonality.Balanced;
        [SerializeField, Range(0f, 1f)] private float _aggressionLevel = 0.5f;
        [SerializeField, Range(0f, 1f)] private float _cautiousLevel = 0.5f;
        [SerializeField, Range(0.1f, 1f)] private float _thinkInterval = 0.2f;
        
        [Header("Debug")]
        [SerializeField] private bool _enableDebugLogging = false;
        [SerializeField] private bool _showDebugGUI = false;

        // Core компоненты
        private AIContext _context;
        private List<IAIBehavior> _availableBehaviors;
        private IAIBehavior _currentBehavior;
        private AIState _currentState = AIState.Initializing;

        // Unity компоненты
        private ICharacter _character;
        private PathProvider _pathProvider;
        
        // Корутины
        private Coroutine _thinkingCoroutine;
        
        // Кэшированные данные
        private IHexGridProvider _grid;
        private List<ICharacter> _allCharacters;

        public AIContext Context => _context;
        public IAIBehavior CurrentBehavior => _currentBehavior;
        public AIState CurrentState => _currentState;
        public BotPersonality Personality => _personality;

        #region Unity Lifecycle

        private void Awake()
        {
            // Получаем компоненты
            _character = GetComponent<ICharacter>();
            _pathProvider = GetComponent<PathProvider>();
            
            if (_character == null)
            {
                Debug.LogError($"[AIBrain] No ICharacter component found on {name}");
                enabled = false;
                return;
            }

            if (_pathProvider == null)
            {
                Debug.LogError($"[AIBrain] No PathProvider component found on {name}");
                enabled = false;
                return;
            }

            // Настраиваем логирование
            AISettings.EnableDebugLogging = _enableDebugLogging;
        }

        private void Start()
        {
            // Инициализация будет вызвана извне через Initialize()
        }

        private void OnEnable()
        {
            if (_context != null && _thinkingCoroutine == null)
            {
                _thinkingCoroutine = StartCoroutine(ThinkingLoop());
            }
        }

        private void OnDisable()
        {
            if (_thinkingCoroutine != null)
            {
                StopCoroutine(_thinkingCoroutine);
                _thinkingCoroutine = null;
            }

            _currentBehavior?.OnExit(_context);
            _currentBehavior = null;
            _currentState = AIState.Dead;
        }

        private void OnGUI()
        {
            if (!_showDebugGUI || _context == null) return;

            var rect = new Rect(10, 10 + (GetInstanceID() % 5) * 150, 300, 140);
            GUI.Box(rect, $"{name} AI Debug");
            
            var labelRect = new Rect(rect.x + 5, rect.y + 20, rect.width - 10, 20);
            GUI.Label(labelRect, $"State: {_currentState}");
            labelRect.y += 20;
            GUI.Label(labelRect, $"Behavior: {_currentBehavior?.Name ?? "None"}");
            labelRect.y += 20;
            GUI.Label(labelRect, $"Personality: {_personality}");
            labelRect.y += 20;
            GUI.Label(labelRect, $"Territory: {_context.GetTerritoryPercentage():P1}");
            labelRect.y += 20;
            GUI.Label(labelRect, $"Enemies nearby: {_context.GetNearbyEnemies().Count}");
            labelRect.y += 20;
            GUI.Label(labelRect, $"Threat Level: {_context.Blackboard.GetFloat("threat_level"):F2}");
        }

        #endregion

        #region Public API

        /// <summary>
        /// Инициализация ИИ (вызывается извне)
        /// </summary>
        public void Initialize(IHexGridProvider grid, List<ICharacter> allCharacters)
        {
            if (grid == null)
            {
                Debug.LogError($"[AIBrain] Grid provider is null for {name}");
                return;
            }

            _grid = grid;
            _allCharacters = allCharacters ?? new List<ICharacter>();

            // Создаём контекст
            _context = new AIContext(_character, _grid, _personality)
            {
                AggressionLevel = _aggressionLevel,
                CautiousLevel = _cautiousLevel
            };

            // Инициализируем поведения
            InitializeBehaviors();

            // Запускаем мышление
            _currentState = AIState.Thinking;
            if (_thinkingCoroutine == null)
            {
                _thinkingCoroutine = StartCoroutine(ThinkingLoop());
            }

            if (_enableDebugLogging)
                Debug.Log($"[AIBrain] {name} initialized with personality: {_personality}");
        }

        /// <summary>
        /// Обновить список всех персонажей (вызывается извне)
        /// </summary>
        public void UpdateCharactersList(List<ICharacter> allCharacters)
        {
            _allCharacters = allCharacters ?? new List<ICharacter>();
            _context?.UpdateEnemies(_allCharacters);
        }

        /// <summary>
        /// Принудительно изменить поведение
        /// </summary>
        public void ForceBehavior(IAIBehavior behavior)
        {
            if (_context == null) return;

            SwitchBehavior(behavior);
        }

        /// <summary>
        /// Остановить ИИ
        /// </summary>
        public void Stop()
        {
            if (_thinkingCoroutine != null)
            {
                StopCoroutine(_thinkingCoroutine);
                _thinkingCoroutine = null;
            }

            _currentBehavior?.OnExit(_context);
            _currentBehavior = null;
            _currentState = AIState.Dead;
        }

        #endregion

        #region Private Methods

        private void InitializeBehaviors()
        {
            _availableBehaviors = new List<IAIBehavior>
            {
                new IdleBehavior(),
                new ExploreBehavior(_pathProvider)
                // Добавим больше поведений позже
            };

            if (_enableDebugLogging)
                Debug.Log($"[AIBrain] {name} initialized with {_availableBehaviors.Count} behaviors");
        }

        private IEnumerator ThinkingLoop()
        {
            while (_currentState != AIState.Dead && enabled)
            {
                try
                {
                    // Проверяем, жив ли персонаж
                    if (_character.State != CharacterState.Alive)
                    {
                        _currentState = AIState.Dead;
                        yield break;
                    }

                    _currentState = AIState.Thinking;

                    // Обновляем контекст
                    _context.UpdateEnemies(_allCharacters);
                    _context.Update();

                    // Выбираем лучшее поведение
                    var bestBehavior = SelectBestBehavior();

                    // Переключаемся на новое поведение, если нужно
                    if (bestBehavior != _currentBehavior)
                    {
                        SwitchBehavior(bestBehavior);
                    }

                    // Выполняем текущее поведение
                    if (_currentBehavior != null)
                    {
                        _currentState = AIState.Acting;
                        var result = _currentBehavior.Execute(_context);

                        // Обрабатываем результат поведения
                        if (result == BehaviorResult.Success || result == BehaviorResult.Failure)
                        {
                            if (_enableDebugLogging)
                                Debug.Log($"[AIBrain] {name} behavior {_currentBehavior.Name} finished with {result}");

                            // Поведение завершено, выберем новое на следующей итерации
                            _currentBehavior.OnExit(_context);
                            _currentBehavior = null;
                        }
                    }
                    else
                    {
                        _currentState = AIState.Waiting;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[AIBrain] Error in thinking loop for {name}: {ex}");
                    _currentState = AIState.Waiting;
                }

                yield return new WaitForSeconds(_thinkInterval);
            }
        }

        private IAIBehavior SelectBestBehavior()
        {
            if (_availableBehaviors == null || _availableBehaviors.Count == 0)
                return null;

            IAIBehavior bestBehavior = null;
            Priority highestPriority = Priority.Low;

            // Простая система приоритетов - выбираем поведение с наивысшим приоритетом,
            // которое может быть выполнено
            foreach (var behavior in _availableBehaviors)
            {
                try
                {
                    if (behavior.CanExecute(_context))
                    {
                        // Если это текущее поведение и оно все еще может выполняться,
                        // даём ему небольшой бонус к приоритету для стабильности
                        var effectivePriority = behavior.Priority;
                        if (behavior == _currentBehavior)
                        {
                            effectivePriority = (Priority)Math.Min((int)effectivePriority + 1, (int)Priority.Critical);
                        }

                        if (effectivePriority > highestPriority || bestBehavior == null)
                        {
                            bestBehavior = behavior;
                            highestPriority = effectivePriority;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[AIBrain] Error checking behavior {behavior.Name}: {ex}");
                }
            }

            // Если ничего не найдено, возвращаем IdleBehavior как fallback
            if (bestBehavior == null)
            {
                bestBehavior = _availableBehaviors.Find(b => b is IdleBehavior);
            }

            return bestBehavior;
        }

        private void SwitchBehavior(IAIBehavior newBehavior)
        {
            if (newBehavior == _currentBehavior) return;

            try
            {
                // Выходим из старого поведения
                if (_currentBehavior != null)
                {
                    _currentBehavior.OnExit(_context);
                    
                    if (_enableDebugLogging)
                        Debug.Log($"[AIBrain] {name} exiting behavior: {_currentBehavior.Name}");
                }

                // Входим в новое поведение
                _currentBehavior = newBehavior;
                
                if (_currentBehavior != null)
                {
                    _currentBehavior.OnEnter(_context);
                    
                    if (_enableDebugLogging)
                        Debug.Log($"[AIBrain] {name} entering behavior: {_currentBehavior.Name}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AIBrain] Error switching behavior for {name}: {ex}");
                _currentBehavior = null;
            }
        }

        #endregion

        #region Inspector Methods (для удобства настройки в редакторе)

        [ContextMenu("Force Idle")]
        private void ForceIdle()
        {
            if (_availableBehaviors != null)
            {
                var idleBehavior = _availableBehaviors.Find(b => b is IdleBehavior);
                if (idleBehavior != null)
                    ForceBehavior(idleBehavior);
            }
        }

        [ContextMenu("Force Explore")]
        private void ForceExplore()
        {
            if (_availableBehaviors != null)
            {
                var exploreBehavior = _availableBehaviors.Find(b => b is ExploreBehavior);
                if (exploreBehavior != null)
                    ForceBehavior(exploreBehavior);
            }
        }

        [ContextMenu("Print Debug Info")]
        private void PrintDebugInfo()
        {
            if (_context != null)
                Debug.Log(_context.GetDebugInfo());
            else
                Debug.Log($"[AIBrain] {name} - Context not initialized");
        }

        #endregion
    }
}