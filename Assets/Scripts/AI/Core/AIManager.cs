using UnityEngine;
using AI.Components;
using AI.Services;

namespace AI.Core
{
    /// <summary>
    /// Новый упрощённый AIManager - координирует AI компоненты и сервисы
    /// Заменяет старый монолитный AIManager (305 строк -> 120 строк)
    /// Интегрирован с новой service-based архитектурой
    /// </summary>
    [RequireComponent(typeof(AIBotRegistry), typeof(AIStatisticsCollector))]
    public class AIManager : MonoBehaviour
    {
        [Header("AI Manager Settings")]
        [SerializeField] private float _updateInterval = 1f;
        [SerializeField] private bool _enableGlobalDebug = false;
        
        [Header("References")]
        [SerializeField] private HexGrid _hexGrid;

        // AI компоненты
        private AIBotRegistry _botRegistry;
        private AIStatisticsCollector _statisticsCollector;
        
        // Управление обновлениями
        private float _lastUpdateTime;

        // События для внешних систем
        public System.Action<System.Collections.Generic.List<ICharacter>> OnCharactersUpdated;

        #region Unity Lifecycle

        private void Awake()
        {
            // Получаем AI компоненты
            _botRegistry = GetComponent<AIBotRegistry>();
            _statisticsCollector = GetComponent<AIStatisticsCollector>();

            if (!ValidateComponents())
            {
                enabled = false;
                return;
            }

            // Ищем HexGrid если не назначен
            if (_hexGrid == null)
            {
                _hexGrid = FindObjectOfType<HexGrid>();
                if (_hexGrid == null)
                {
                    Debug.LogError("[AIManager] No HexGrid found in scene!");
                    enabled = false;
                    return;
                }
            }

            // Проверяем доступность сервисов
            if (AIServiceContainer.Instance == null)
            {
                Debug.LogWarning("[AIManager] AIServiceContainer not found - some features may be limited");
            }
        }

        private void Start()
        {
            // Подписываемся на события компонентов
            if (_botRegistry != null)
            {
                _botRegistry.OnCharactersUpdated += OnInternalCharactersUpdated;
            }

            // Инициализируем систему
            InitializeAISystem();

            if (_enableGlobalDebug)
            {
                Debug.Log($"[AIManager] Initialized with {_botRegistry.RegisteredBots.Count} bots " +
                         $"and {_botRegistry.AllCharacters.Count} characters");
            }
        }

        private void Update()
        {
            // Периодические обновления
            if (Time.time - _lastUpdateTime >= _updateInterval)
            {
                UpdateAISystem();
                _lastUpdateTime = Time.time;
            }
        }

        private void OnDestroy()
        {
            // Отписываемся от событий
            if (_botRegistry != null)
            {
                _botRegistry.OnCharactersUpdated -= OnInternalCharactersUpdated;
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Зарегистрировать нового AI бота
        /// </summary>
        public bool RegisterBot(NewAIEnemy aiBot)
        {
            if (_botRegistry == null) return false;

            bool registered = _botRegistry.RegisterBot(aiBot);
            
            if (registered && _hexGrid != null)
            {
                // Сразу инициализируем нового бота
                try
                {
                    aiBot.InitializeAI(_hexGrid, _botRegistry.AllCharacters);
                    
                    if (_enableGlobalDebug)
                        Debug.Log($"[AIManager] Bot {aiBot.name} registered and initialized");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[AIManager] Failed to initialize registered bot {aiBot.name}: {ex}");
                }
            }

            return registered;
        }

        /// <summary>
        /// Отменить регистрацию AI бота
        /// </summary>
        public bool UnregisterBot(NewAIEnemy aiBot)
        {
            return _botRegistry?.UnregisterBot(aiBot) ?? false;
        }

        /// <summary>
        /// Принудительно обновить всю систему
        /// </summary>
        public void RefreshSystem()
        {
            _botRegistry?.RefreshCharacterLists();
            InitializeAISystem();
            
            if (_enableGlobalDebug)
                Debug.Log("[AIManager] System refreshed manually");
        }

        /// <summary>
        /// Установить интервал обновления
        /// </summary>
        public void SetUpdateInterval(float interval)
        {
            _updateInterval = Mathf.Clamp(interval, 0.1f, 5f);
            
            if (_enableGlobalDebug)
                Debug.Log($"[AIManager] Update interval set to {_updateInterval}s");
        }

        /// <summary>
        /// Получить текущую статистику системы
        /// </summary>
        public System.Collections.Generic.IReadOnlyDictionary<string, object> GetStatistics()
        {
            return _statisticsCollector?.CurrentStatistics;
        }

        /// <summary>
        /// Проверить статус AI сервисов
        /// </summary>
        public bool AreServicesReady()
        {
            return AIServiceContainer.Instance?.AreServicesReady() ?? false;
        }

        #endregion

        #region Private Methods

        private bool ValidateComponents()
        {
            if (_botRegistry == null)
            {
                Debug.LogError("[AIManager] No AIBotRegistry component found!");
                return false;
            }

            if (_statisticsCollector == null)
            {
                Debug.LogError("[AIManager] No AIStatisticsCollector component found!");
                return false;
            }

            return true;
        }

        private void InitializeAISystem()
        {
            if (_botRegistry == null || _hexGrid == null) return;

            // Инициализируем всех ботов
            int initializedCount = _botRegistry.InitializeAllBots(_hexGrid);

            if (_enableGlobalDebug)
            {
                var servicesStatus = AreServicesReady() ? "ready" : "not ready";
                Debug.Log($"[AIManager] AI System initialized: {initializedCount} bots, services {servicesStatus}");
            }
        }

        private void UpdateAISystem()
        {
            if (_botRegistry == null) return;

            // Проверяем изменения в персонажах
            if (_botRegistry.HasCharacterCountChanged())
            {
                _botRegistry.RefreshCharacterLists();
                
                if (_enableGlobalDebug)
                    Debug.Log("[AIManager] Character count changed, lists refreshed");
            }

            // Обновляем всех ботов
            _botRegistry.UpdateAllBots();
        }

        private void OnInternalCharactersUpdated(System.Collections.Generic.List<ICharacter> characters)
        {
            // Передаём событие дальше
            OnCharactersUpdated?.Invoke(characters);

            if (_enableGlobalDebug)
                Debug.Log($"[AIManager] Characters updated: {characters.Count} total");
        }

        #endregion

        #region Inspector Tools

        [ContextMenu("Refresh AI System")]
        private void InspectorRefreshSystem()
        {
            RefreshSystem();
        }

        [ContextMenu("Print System Status")]
        private void InspectorPrintStatus()
        {
            var stats = GetStatistics();
            var report = "[AIManager] System Status:\n";
            
            if (stats != null)
            {
                foreach (var kvp in stats)
                {
                    report += $"  {kvp.Key}: {kvp.Value}\n";
                }
            }
            
            report += $"  Services Ready: {AreServicesReady()}\n";
            report += $"  Update Interval: {_updateInterval}s\n";
            
            Debug.Log(report);
        }

        [ContextMenu("Test Service Integration")]
        private void InspectorTestServices()
        {
            var container = AIServiceContainer.Instance;
            if (container == null)
            {
                Debug.LogError("[AIManager] No AIServiceContainer found!");
                return;
            }

            var report = "[AIManager] Service Integration Test:\n";
            report += $"  Container Available: ✓\n";
            report += $"  Services Ready: {(container.AreServicesReady() ? "✓" : "✗")}\n";
            
            // Тестируем отдельные сервисы
            if (AIServiceContainer.BehaviorUtils != null)
                report += "  BehaviorUtils: ✓\n";
            else
                report += "  BehaviorUtils: ✗\n";
                
            if (AIServiceContainer.Pathfinding != null)
                report += "  Pathfinding: ✓\n";
            else
                report += "  Pathfinding: ✗\n";
                
            if (AIServiceContainer.TerritoryAnalysis != null)
                report += "  TerritoryAnalysis: ✓\n";
            else
                report += "  TerritoryAnalysis: ✗\n";
            
            Debug.Log(report);
        }

        [ContextMenu("Toggle Global Debug")]
        private void InspectorToggleDebug()
        {
            _enableGlobalDebug = !_enableGlobalDebug;
            Debug.Log($"[AIManager] Global debug {(_enableGlobalDebug ? "enabled" : "disabled")}");
        }

        #endregion
    }
}