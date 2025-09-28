using System.Collections.Generic;
using UnityEngine;
using AI.Components;
using AI.Services;

namespace AI.Core
{
    /// <summary>
    /// Новый упрощённый AIBrain - координирует работу AI компонентов
    /// Заменяет старый монолитный AIBrain (386 строк -> 120 строк)
    /// </summary>
    [RequireComponent(typeof(ICharacter), typeof(PathProvider))]
    [RequireComponent(typeof(AIBehaviorSelector), typeof(AIDebugRenderer), typeof(AILifecycleManager))]
    public class AIBrainNew : MonoBehaviour
    {
        [Header("AI Settings")]
        [SerializeField] private BotPersonality _personality = BotPersonality.Balanced;
        [SerializeField, Range(0f, 1f)] private float _aggressionLevel = 0.5f;
        [SerializeField, Range(0f, 1f)] private float _cautiousLevel = 0.5f;
        
        [Header("Debug")]
        [SerializeField] private bool _enableDebugLogging = false;

        // Core компоненты
        private AIContext _context;
        private ICharacter _character;
        private PathProvider _pathProvider;

        // AI компоненты
        private AIBehaviorSelector _behaviorSelector;
        private AIDebugRenderer _debugRenderer;
        private AILifecycleManager _lifecycleManager;

        // Кэшированные данные
        private IHexGridProvider _grid;
        private List<ICharacter> _allCharacters;

        // Public свойства
        public AIContext Context => _context;
        public AIState CurrentState => _lifecycleManager?.CurrentState ?? AIState.Dead;
        public BotPersonality Personality => _personality;

        #region Unity Lifecycle

        private void Awake()
        {
            // Получаем обязательные компоненты
            _character = GetComponent<ICharacter>();
            _pathProvider = GetComponent<PathProvider>();

            // Получаем AI компоненты
            _behaviorSelector = GetComponent<AIBehaviorSelector>();
            _debugRenderer = GetComponent<AIDebugRenderer>();
            _lifecycleManager = GetComponent<AILifecycleManager>();

            // Проверяем что всё на месте
            if (!ValidateComponents())
            {
                enabled = false;
                return;
            }

            // Настраиваем события
            if (_lifecycleManager != null)
            {
                _lifecycleManager.OnThinkingCycle += OnThinkingCycle;
                _lifecycleManager.OnStateChanged += OnStateChanged;
            }

            if (_enableDebugLogging)
                Debug.Log($"[AIBrainNew] {name} components initialized");
        }

        private void Start()
        {
            // Инициализация вызывается извне через Initialize()
        }

        private void OnDestroy()
        {
            // Отписываемся от событий
            if (_lifecycleManager != null)
            {
                _lifecycleManager.OnThinkingCycle -= OnThinkingCycle;
                _lifecycleManager.OnStateChanged -= OnStateChanged;
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Инициализация AI системы
        /// </summary>
        public void Initialize(IHexGridProvider grid, List<ICharacter> allCharacters)
        {
            if (grid == null)
            {
                Debug.LogError($"[AIBrainNew] Grid provider is null for {name}");
                return;
            }

            _grid = grid;
            _allCharacters = allCharacters ?? new List<ICharacter>();

            // Создаём контекст AI
            _context = new AIContext(_character, _grid, _personality)
            {
                AggressionLevel = _aggressionLevel,
                CautiousLevel = _cautiousLevel
            };

            // Инициализируем компоненты
            InitializeComponents();

            // Запускаем жизненный цикл
            _lifecycleManager.Initialize();
            _lifecycleManager.StartThinking();

            if (_enableDebugLogging)
                Debug.Log($"[AIBrainNew] {name} initialized with personality: {_personality}");
        }

        /// <summary>
        /// Обновить список персонажей
        /// </summary>
        public void UpdateCharactersList(List<ICharacter> allCharacters)
        {
            _allCharacters = allCharacters ?? new List<ICharacter>();
            _context?.UpdateEnemies(_allCharacters);
        }

        /// <summary>
        /// Принудительно установить поведение (для отладки)
        /// </summary>
        public void ForceBehavior(string behaviorName)
        {
            if (_context == null || _behaviorSelector == null) return;

            var behavior = behaviorName.ToLower() switch
            {
                "idle" => _behaviorSelector.GetBehavior<IdleBehavior>(),
                "explore" => _behaviorSelector.GetBehavior<ExploreBehaviorNew>(),
                _ => null
            };

            if (behavior != null)
            {
                _behaviorSelector.ForceBehavior(behavior, _context);
            }
            else
            {
                Debug.LogWarning($"[AIBrainNew] Unknown behavior: {behaviorName}");
            }
        }

        /// <summary>
        /// Остановить AI
        /// </summary>
        public void Stop()
        {
            _lifecycleManager?.StopThinking();
            _behaviorSelector?.StopCurrentBehavior(_context);
        }

        #endregion

        #region Private Methods

        private bool ValidateComponents()
        {
            if (_character == null)
            {
                Debug.LogError($"[AIBrainNew] No ICharacter component found on {name}");
                return false;
            }

            if (_pathProvider == null)
            {
                Debug.LogError($"[AIBrainNew] No PathProvider component found on {name}");
                return false;
            }

            if (_behaviorSelector == null)
            {
                Debug.LogError($"[AIBrainNew] No AIBehaviorSelector component found on {name}");
                return false;
            }

            if (_debugRenderer == null)
            {
                Debug.LogError($"[AIBrainNew] No AIDebugRenderer component found on {name}");
                return false;
            }

            if (_lifecycleManager == null)
            {
                Debug.LogError($"[AIBrainNew] No AILifecycleManager component found on {name}");
                return false;
            }

            return true;
        }

        private void InitializeComponents()
        {
            // Инициализируем селектор поведений
            _behaviorSelector.InitializeBehaviors(_pathProvider, _personality);

            // Настраиваем отладочный рендерер
            _debugRenderer.SetDebugGUIEnabled(_enableDebugLogging);
        }

        private void OnThinkingCycle()
        {
            if (_context == null || _behaviorSelector == null) return;

            try
            {
                // Обновляем контекст
                _context.UpdateEnemies(_allCharacters);

                // Выбираем лучшее поведение
                var bestBehavior = _behaviorSelector.SelectBestBehavior(_context);
                
                if (bestBehavior != null)
                {
                    // Переключаемся на новое поведение если нужно
                    _behaviorSelector.SwitchBehavior(bestBehavior, _context);

                    // Выполняем текущее поведение
                    var result = bestBehavior.Execute(_context);
                    
                    if (result == BehaviorResult.Failure && _enableDebugLogging)
                    {
                        Debug.LogWarning($"[AIBrainNew] Behavior {bestBehavior.Name} failed for {name}");
                    }
                }

                // Обновляем отладочную информацию
                UpdateDebugInfo();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AIBrainNew] Error in thinking cycle for {name}: {ex}");
                _lifecycleManager.ChangeState(AIState.Dead);
            }
        }

        private void OnStateChanged(AIState newState)
        {
            if (_enableDebugLogging)
                Debug.Log($"[AIBrainNew] {name} state changed to {newState}");

            UpdateDebugInfo();
        }

        private void UpdateDebugInfo()
        {
            if (_debugRenderer != null && _context != null)
            {
                var behaviorName = _behaviorSelector?.CurrentBehavior?.Name ?? "None";
                _debugRenderer.SetDebugData(_context, CurrentState, behaviorName, _personality);
            }
        }

        #endregion

        #region Inspector Tools

        [ContextMenu("Force Idle")]
        private void ForceIdle() => ForceBehavior("idle");

        [ContextMenu("Force Explore")]
        private void ForceExplore() => ForceBehavior("explore");

        [ContextMenu("Print Debug Info")]
        private void PrintDebugInfo()
        {
            if (_context != null)
                Debug.Log(_context.GetDebugInfo());
            else
                Debug.Log($"[AIBrainNew] {name} - Context not initialized");
        }

        [ContextMenu("Toggle Debug GUI")]
        private void ToggleDebugGUI()
        {
            _enableDebugLogging = !_enableDebugLogging;
            _debugRenderer?.SetDebugGUIEnabled(_enableDebugLogging);
            Debug.Log($"[AIBrainNew] Debug GUI {(_enableDebugLogging ? "enabled" : "disabled")} for {name}");
        }

        #endregion
    }
}