using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AI.Core;

namespace AI.Components
{
    /// <summary>
    /// Компонент для регистрации и управления AI ботами
    /// Выделен из AIManager для модульности
    /// </summary>
    public class AIBotRegistry : MonoBehaviour
    {
        [Header("Registry Settings")]
        [SerializeField] private bool _enableRegistryLogging = false;
        [SerializeField] private bool _autoRegisterOnStart = true;

        private List<NewAIEnemy> _registeredBots = new List<NewAIEnemy>();
        private List<ICharacter> _allCharacters = new List<ICharacter>();
        private int _totalInitializedBots = 0;

        // События для уведомления других компонентов
        public event Action<NewAIEnemy> OnBotRegistered;
        public event Action<NewAIEnemy> OnBotUnregistered;
        public event Action<List<ICharacter>> OnCharactersUpdated;

        public IReadOnlyList<NewAIEnemy> RegisteredBots => _registeredBots.AsReadOnly();
        public IReadOnlyList<ICharacter> AllCharacters => _allCharacters.AsReadOnly();
        public int TotalInitializedBots => _totalInitializedBots;

        #region Unity Lifecycle

        private void Start()
        {
            if (_autoRegisterOnStart)
            {
                RefreshCharacterLists();
                
                if (_enableRegistryLogging)
                    Debug.Log($"[AIBotRegistry] Auto-registered {_registeredBots.Count} bots and {_allCharacters.Count} characters");
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Зарегистрировать нового AI бота
        /// </summary>
        public bool RegisterBot(NewAIEnemy aiBot)
        {
            if (aiBot == null)
            {
                Debug.LogWarning("[AIBotRegistry] Trying to register null bot");
                return false;
            }

            if (_registeredBots.Contains(aiBot))
            {
                if (_enableRegistryLogging)
                    Debug.LogWarning($"[AIBotRegistry] Bot {aiBot.name} already registered");
                return false;
            }

            _registeredBots.Add(aiBot);
            OnBotRegistered?.Invoke(aiBot);

            if (_enableRegistryLogging)
                Debug.Log($"[AIBotRegistry] Registered bot: {aiBot.name} (Total: {_registeredBots.Count})");

            return true;
        }

        /// <summary>
        /// Отменить регистрацию AI бота
        /// </summary>
        public bool UnregisterBot(NewAIEnemy aiBot)
        {
            if (aiBot == null) return false;

            bool removed = _registeredBots.Remove(aiBot);
            if (removed)
            {
                _totalInitializedBots = Mathf.Max(0, _totalInitializedBots - 1);
                OnBotUnregistered?.Invoke(aiBot);

                if (_enableRegistryLogging)
                    Debug.Log($"[AIBotRegistry] Unregistered bot: {aiBot.name} (Total: {_registeredBots.Count})");
            }

            return removed;
        }

        /// <summary>
        /// Обновить списки всех персонажей и ботов
        /// </summary>
        public void RefreshCharacterLists()
        {
            var oldBotCount = _registeredBots.Count;
            var oldCharacterCount = _allCharacters.Count;

            // Собираем всех AI ботов
            _registeredBots.Clear();
            var aiEnemies = FindObjectsOfType<NewAIEnemy>();
            _registeredBots.AddRange(aiEnemies);

            // Собираем всех персонажей (включая игрока)
            _allCharacters.Clear();
            var allCharacterComponents = FindObjectsOfType<CharacterAbstract>();
            _allCharacters.AddRange(allCharacterComponents.Cast<ICharacter>());

            // Уведомляем о изменениях
            OnCharactersUpdated?.Invoke(_allCharacters);

            if (_enableRegistryLogging || oldBotCount != _registeredBots.Count || oldCharacterCount != _allCharacters.Count)
            {
                Debug.Log($"[AIBotRegistry] Lists refreshed: {_registeredBots.Count} bots (+{_registeredBots.Count - oldBotCount}), " +
                         $"{_allCharacters.Count} characters (+{_allCharacters.Count - oldCharacterCount})");
            }
        }

        /// <summary>
        /// Инициализировать всех зарегистрированных ботов
        /// </summary>
        public int InitializeAllBots(IHexGridProvider grid)
        {
            if (grid == null)
            {
                Debug.LogError("[AIBotRegistry] Cannot initialize bots - no grid provider");
                return 0;
            }

            _totalInitializedBots = 0;
            var failed = 0;

            foreach (var bot in _registeredBots)
            {
                if (bot != null)
                {
                    try
                    {
                        bot.InitializeAI(grid, _allCharacters);
                        _totalInitializedBots++;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[AIBotRegistry] Failed to initialize bot {bot.name}: {ex}");
                        failed++;
                    }
                }
            }

            if (_enableRegistryLogging || failed > 0)
            {
                Debug.Log($"[AIBotRegistry] Initialized {_totalInitializedBots}/{_registeredBots.Count} bots" +
                         (failed > 0 ? $" ({failed} failed)" : ""));
            }

            return _totalInitializedBots;
        }

        /// <summary>
        /// Обновить информацию о персонажах для всех ботов
        /// </summary>
        public void UpdateAllBots()
        {
            var updated = 0;
            var failed = 0;

            foreach (var bot in _registeredBots)
            {
                if (bot != null)
                {
                    try
                    {
                        bot.UpdateCharactersList(_allCharacters);
                        updated++;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[AIBotRegistry] Error updating bot {bot.name}: {ex}");
                        failed++;
                    }
                }
            }

            if (_enableRegistryLogging)
            {
                Debug.Log($"[AIBotRegistry] Updated {updated} bots" + 
                         (failed > 0 ? $" ({failed} failed)" : ""));
            }
        }

        /// <summary>
        /// Найти бота по имени
        /// </summary>
        public NewAIEnemy FindBotByName(string name)
        {
            return _registeredBots.FirstOrDefault(bot => bot != null && bot.name == name);
        }

        /// <summary>
        /// Получить ботов определённой личности
        /// </summary>
        public List<NewAIEnemy> GetBotsByPersonality(BotPersonality personality)
        {
            return _registeredBots.Where(bot => bot != null && bot.Personality == personality).ToList();
        }

        /// <summary>
        /// Проверить, изменилось ли количество персонажей
        /// </summary>
        public bool HasCharacterCountChanged()
        {
            var currentCount = FindObjectsOfType<CharacterAbstract>().Length;
            return currentCount != _allCharacters.Count;
        }

        #endregion

        #region Inspector Tools

        [ContextMenu("Refresh Lists")]
        private void InspectorRefreshLists()
        {
            RefreshCharacterLists();
        }

        [ContextMenu("Print Registry Stats")]
        private void InspectorPrintStats()
        {
            var report = "[AIBotRegistry] Statistics:\n";
            report += $"  Registered Bots: {_registeredBots.Count}\n";
            report += $"  Initialized Bots: {_totalInitializedBots}\n";
            report += $"  Total Characters: {_allCharacters.Count}\n";

            // Статистика по личностям
            var personalityStats = _registeredBots
                .Where(bot => bot != null)
                .GroupBy(bot => bot.Personality)
                .ToDictionary(g => g.Key, g => g.Count());

            if (personalityStats.Count > 0)
            {
                report += "  Personalities:\n";
                foreach (var kvp in personalityStats)
                {
                    report += $"    {kvp.Key}: {kvp.Value}\n";
                }
            }

            Debug.Log(report);
        }

        [ContextMenu("Clear Registry")]
        private void InspectorClearRegistry()
        {
            _registeredBots.Clear();
            _totalInitializedBots = 0;
            Debug.Log("[AIBotRegistry] Registry cleared");
        }

        #endregion
    }
}