using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AI.Core;
using AI.Services;

namespace AI.Components
{
    /// <summary>
    /// Компонент для сбора и анализа статистики AI системы
    /// Выделен из AIManager, использует новые сервисы
    /// </summary>
    public class AIStatisticsCollector : MonoBehaviour
    {
        [Header("Statistics Settings")]
        [SerializeField] private bool _enableStatisticsLogging = false;
        [SerializeField] private float _statisticsUpdateInterval = 10f;
        [SerializeField] private bool _showStatisticsGUI = false;

        private AIBotRegistry _botRegistry;
        private float _lastStatisticsUpdate;
        private Dictionary<string, object> _currentStatistics;

        // Временные статистики
        private Dictionary<BotPersonality, int> _personalityDistribution = new();
        private Dictionary<string, int> _behaviorUsageStats = new();
        private Dictionary<AIState, int> _stateDistribution = new();

        public IReadOnlyDictionary<string, object> CurrentStatistics => _currentStatistics;

        #region Unity Lifecycle

        private void Awake()
        {
            _botRegistry = GetComponent<AIBotRegistry>();
            if (_botRegistry == null)
            {
                Debug.LogError("[AIStatisticsCollector] No AIBotRegistry component found!");
                enabled = false;
                return;
            }

            _currentStatistics = new Dictionary<string, object>();
        }

        private void Start()
        {
            // Подписываемся на события реестра
            _botRegistry.OnBotRegistered += OnBotRegistered;
            _botRegistry.OnBotUnregistered += OnBotUnregistered;
            _botRegistry.OnCharactersUpdated += OnCharactersUpdated;

            // Собираем начальную статистику
            UpdateStatistics();
        }

        private void Update()
        {
            if (Time.time - _lastStatisticsUpdate >= _statisticsUpdateInterval)
            {
                UpdateStatistics();
                _lastStatisticsUpdate = Time.time;
            }
        }

        private void OnDestroy()
        {
            // Отписываемся от событий
            if (_botRegistry != null)
            {
                _botRegistry.OnBotRegistered -= OnBotRegistered;
                _botRegistry.OnBotUnregistered -= OnBotUnregistered;
                _botRegistry.OnCharactersUpdated -= OnCharactersUpdated;
            }
        }

        private void OnGUI()
        {
            if (!_showStatisticsGUI || _currentStatistics == null) return;

            var rect = new Rect(Screen.width - 320, 10, 310, 300);
            GUI.Box(rect, "AI Statistics");

            var labelRect = new Rect(rect.x + 5, rect.y + 20, rect.width - 10, 20);

            // Основная статистика
            GUI.Label(labelRect, $"Total Bots: {_currentStatistics.GetValueOrDefault("TotalBots", 0)}");
            labelRect.y += 20;
            GUI.Label(labelRect, $"Initialized: {_currentStatistics.GetValueOrDefault("InitializedBots", 0)}");
            labelRect.y += 20;
            GUI.Label(labelRect, $"Characters: {_currentStatistics.GetValueOrDefault("TotalCharacters", 0)}");
            labelRect.y += 20;

            // Проверяем доступность сервисов
            if (AIServiceContainer.Instance != null)
            {
                var servicesReady = AIServiceContainer.Instance.AreServicesReady();
                GUI.Label(labelRect, $"Services: {(servicesReady ? "✓ Ready" : "✗ Not Ready")}");
                labelRect.y += 20;
            }

            labelRect.y += 10;

            // Распределение личностей
            GUI.Label(labelRect, "Personalities:");
            labelRect.y += 20;
            foreach (var kvp in _personalityDistribution)
            {
                GUI.Label(labelRect, $"  {kvp.Key}: {kvp.Value}");
                labelRect.y += 15;
            }

            labelRect.y += 10;

            // Статистика поведений
            if (_behaviorUsageStats.Count > 0)
            {
                GUI.Label(labelRect, "Behaviors:");
                labelRect.y += 20;
                foreach (var kvp in _behaviorUsageStats.Take(3)) // Показываем топ 3
                {
                    GUI.Label(labelRect, $"  {kvp.Key}: {kvp.Value}");
                    labelRect.y += 15;
                }
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Принудительно обновить статистику
        /// </summary>
        public void UpdateStatistics()
        {
            if (_botRegistry == null) return;

            try
            {
                _currentStatistics.Clear();
                
                // Основная статистика
                _currentStatistics["TotalBots"] = _botRegistry.RegisteredBots.Count;
                _currentStatistics["InitializedBots"] = _botRegistry.TotalInitializedBots;
                _currentStatistics["TotalCharacters"] = _botRegistry.AllCharacters.Count;
                _currentStatistics["UpdateInterval"] = _statisticsUpdateInterval;

                // Проверяем статус сервисов
                _currentStatistics["ServicesAvailable"] = AIServiceContainer.Instance != null;
                _currentStatistics["ServicesReady"] = AIServiceContainer.Instance?.AreServicesReady() ?? false;

                // Собираем детальную статистику
                CollectPersonalityStatistics();
                CollectBehaviorStatistics();
                CollectStateStatistics();

                _currentStatistics["PersonalityDistribution"] = new Dictionary<BotPersonality, int>(_personalityDistribution);
                _currentStatistics["BehaviorUsage"] = new Dictionary<string, int>(_behaviorUsageStats);
                _currentStatistics["StateDistribution"] = new Dictionary<AIState, int>(_stateDistribution);

                if (_enableStatisticsLogging)
                {
                    Debug.Log($"[AIStatisticsCollector] Statistics updated: {_currentStatistics["TotalBots"]} bots, " +
                             $"{_currentStatistics["TotalCharacters"]} characters");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AIStatisticsCollector] Error updating statistics: {ex}");
            }
        }

        /// <summary>
        /// Получить статистику по конкретному ключу
        /// </summary>
        public T GetStatistic<T>(string key, T defaultValue = default)
        {
            if (_currentStatistics.TryGetValue(key, out var value) && value is T)
            {
                return (T)value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Экспортировать статистику в JSON формате
        /// </summary>
        public string ExportStatisticsToJson()
        {
            try
            {
                return JsonUtility.ToJson(_currentStatistics, true);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AIStatisticsCollector] Error exporting statistics: {ex}");
                return "{}";
            }
        }

        #endregion

        #region Private Methods

        private void CollectPersonalityStatistics()
        {
            _personalityDistribution.Clear();

            foreach (var bot in _botRegistry.RegisteredBots)
            {
                if (bot != null)
                {
                    var personality = bot.Personality;
                    _personalityDistribution[personality] = _personalityDistribution.GetValueOrDefault(personality, 0) + 1;
                }
            }
        }

        private void CollectBehaviorStatistics()
        {
            _behaviorUsageStats.Clear();

            // Используем AIServiceContainer для доступа к поведениям
            if (AIServiceContainer.Instance?.AreServicesReady() == true)
            {
                foreach (var bot in _botRegistry.RegisteredBots)
                {
                    if (bot != null && bot.CurrentBehavior != null)
                    {
                        var behaviorName = bot.CurrentBehavior.Name;
                        _behaviorUsageStats[behaviorName] = _behaviorUsageStats.GetValueOrDefault(behaviorName, 0) + 1;
                    }
                }
            }
        }

        private void CollectStateStatistics()
        {
            _stateDistribution.Clear();

            foreach (var bot in _botRegistry.RegisteredBots)
            {
                if (bot != null)
                {
                    var state = bot.CurrentState;
                    _stateDistribution[state] = _stateDistribution.GetValueOrDefault(state, 0) + 1;
                }
            }
        }

        private void OnBotRegistered(NewAIEnemy bot)
        {
            if (_enableStatisticsLogging)
                Debug.Log($"[AIStatisticsCollector] Bot registered: {bot.name}");
            
            // Обновляем статистику при добавлении бота
            UpdateStatistics();
        }

        private void OnBotUnregistered(NewAIEnemy bot)
        {
            if (_enableStatisticsLogging)
                Debug.Log($"[AIStatisticsCollector] Bot unregistered: {bot.name}");
            
            // Обновляем статистику при удалении бота
            UpdateStatistics();
        }

        private void OnCharactersUpdated(List<ICharacter> characters)
        {
            if (_enableStatisticsLogging)
                Debug.Log($"[AIStatisticsCollector] Characters updated: {characters.Count} total");
        }

        #endregion

        #region Inspector Tools

        [ContextMenu("Update Statistics")]
        private void InspectorUpdateStatistics()
        {
            UpdateStatistics();
            Debug.Log("[AIStatisticsCollector] Statistics updated from inspector");
        }

        [ContextMenu("Print Full Statistics")]
        private void InspectorPrintStatistics()
        {
            UpdateStatistics();
            
            var report = "[AIStatisticsCollector] Full Statistics:\n";
            foreach (var kvp in _currentStatistics)
            {
                report += $"  {kvp.Key}: {kvp.Value}\n";
            }
            
            Debug.Log(report);
        }

        [ContextMenu("Export Statistics to Console")]
        private void InspectorExportStatistics()
        {
            var json = ExportStatisticsToJson();
            Debug.Log($"[AIStatisticsCollector] Exported Statistics:\n{json}");
        }

        [ContextMenu("Toggle Statistics GUI")]
        private void InspectorToggleGUI()
        {
            _showStatisticsGUI = !_showStatisticsGUI;
            Debug.Log($"[AIStatisticsCollector] Statistics GUI {(_showStatisticsGUI ? "enabled" : "disabled")}");
        }

        #endregion
    }
}