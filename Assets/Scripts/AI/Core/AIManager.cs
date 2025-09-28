using System.Collections.Generic;
using UnityEngine;
using AI.Core;
using System.Linq;

/// <summary>
/// Центральный менеджер для управления всеми AI ботами в игре
/// Обновляет информацию о персонажах, координирует AI, собирает статистику
/// </summary>
public class AIManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _updateInterval = 1f;
    [SerializeField] private bool _enableGlobalDebug = false;
    
    [Header("References")]
    [SerializeField] private HexGrid _hexGrid;
    
    // Список всех AI ботов и персонажей
    private List<NewAIEnemy> _aiBots = new List<NewAIEnemy>();
    private List<ICharacter> _allCharacters = new List<ICharacter>();
    
    // Статистика и отладка
    private float _lastUpdateTime;
    private int _totalInitializedBots = 0;
    
    // События для уведомления других систем
    public System.Action<List<ICharacter>> OnCharactersUpdated;

    #region Unity Lifecycle

    private void Awake()
    {
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
    }

    private void Start()
    {
        // Собираем всех AI ботов и персонажей
        RefreshCharacterLists();
        
        // Инициализируем всех ботов
        InitializeAllBots();
        
        Debug.Log($"[AIManager] Initialized with {_aiBots.Count} AI bots and {_allCharacters.Count} total characters");
    }

    private void Update()
    {
        // Периодически обновляем информацию
        if (Time.time - _lastUpdateTime >= _updateInterval)
        {
            UpdateAIBots();
            _lastUpdateTime = Time.time;
        }
    }

    private void OnGUI()
    {
        if (!_enableGlobalDebug) return;

        var rect = new Rect(Screen.width - 320, 10, 310, 200);
        GUI.Box(rect, "AI Manager Debug");
        
        var labelRect = new Rect(rect.x + 5, rect.y + 20, rect.width - 10, 20);
        
        GUI.Label(labelRect, $"Total Characters: {_allCharacters.Count}");
        labelRect.y += 20;
        GUI.Label(labelRect, $"AI Bots: {_aiBots.Count}");
        labelRect.y += 20;
        GUI.Label(labelRect, $"Initialized Bots: {_totalInitializedBots}");
        labelRect.y += 20;
        GUI.Label(labelRect, $"Update Interval: {_updateInterval}s");
        labelRect.y += 20;
        
        // Показываем статус каждого бота
        GUI.Label(labelRect, "Bot Status:");
        labelRect.y += 20;
        
        for (int i = 0; i < Mathf.Min(_aiBots.Count, 5); i++) // Показываем максимум 5 ботов
        {
            var bot = _aiBots[i];
            if (bot != null)
            {
                var status = bot.GetAIStatus();
                var shortStatus = status.Length > 30 ? status.Substring(0, 30) + "..." : status;
                GUI.Label(labelRect, $"{bot.name}: {shortStatus}");
                labelRect.y += 15;
            }
        }
    }

    #endregion

    #region Public API

    /// <summary>
    /// Добавить нового AI бота в систему
    /// </summary>
    public void RegisterAIBot(NewAIEnemy aiBot)
    {
        if (aiBot == null) return;

        if (!_aiBots.Contains(aiBot))
        {
            _aiBots.Add(aiBot);
            
            // Если система уже инициализирована, сразу инициализируем нового бота
            if (_hexGrid != null && _allCharacters.Count > 0)
            {
                aiBot.InitializeAI(_hexGrid, _allCharacters);
                _totalInitializedBots++;
            }
            
            Debug.Log($"[AIManager] Registered AI bot: {aiBot.name}");
        }
    }

    /// <summary>
    /// Удалить AI бота из системы
    /// </summary>
    public void UnregisterAIBot(NewAIEnemy aiBot)
    {
        if (aiBot == null) return;

        if (_aiBots.Remove(aiBot))
        {
            _totalInitializedBots = Mathf.Max(0, _totalInitializedBots - 1);
            Debug.Log($"[AIManager] Unregistered AI bot: {aiBot.name}");
        }
    }

    /// <summary>
    /// Принудительно обновить списки персонажей
    /// </summary>
    public void RefreshCharacterLists()
    {
        // Собираем всех AI ботов
        _aiBots.Clear();
        var aiEnemies = FindObjectsOfType<NewAIEnemy>();
        _aiBots.AddRange(aiEnemies);

        // Собираем всех персонажей (включая игрока)
        _allCharacters.Clear();
        var allCharacterComponents = FindObjectsOfType<CharacterAbstract>();
        _allCharacters.AddRange(allCharacterComponents.Cast<ICharacter>());

        Debug.Log($"[AIManager] Refreshed lists: {_aiBots.Count} AI bots, {_allCharacters.Count} total characters");
    }

    /// <summary>
    /// Получить статистику всех ботов
    /// </summary>
    public Dictionary<string, object> GetAIStatistics()
    {
        var stats = new Dictionary<string, object>
        {
            ["TotalBots"] = _aiBots.Count,
            ["InitializedBots"] = _totalInitializedBots,
            ["TotalCharacters"] = _allCharacters.Count,
            ["UpdateInterval"] = _updateInterval
        };

        // Добавляем информацию о персональностях ботов
        var personalityStats = new Dictionary<BotPersonality, int>();
        foreach (var bot in _aiBots)
        {
            if (bot != null)
            {
                var personality = bot.Personality;
                personalityStats[personality] = personalityStats.GetValueOrDefault(personality, 0) + 1;
            }
        }
        stats["PersonalityDistribution"] = personalityStats;

        return stats;
    }

    /// <summary>
    /// Установить интервал обновления для всех ботов
    /// </summary>
    public void SetUpdateInterval(float interval)
    {
        _updateInterval = Mathf.Clamp(interval, 0.1f, 5f);
        Debug.Log($"[AIManager] Update interval set to {_updateInterval}s");
    }

    #endregion

    #region Private Methods

    private void InitializeAllBots()
    {
        if (_hexGrid == null)
        {
            Debug.LogError("[AIManager] Cannot initialize bots - no HexGrid available");
            return;
        }

        _totalInitializedBots = 0;

        foreach (var bot in _aiBots)
        {
            if (bot != null)
            {
                try
                {
                    bot.InitializeAI(_hexGrid, _allCharacters);
                    _totalInitializedBots++;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[AIManager] Failed to initialize bot {bot.name}: {ex}");
                }
            }
        }

        Debug.Log($"[AIManager] Initialized {_totalInitializedBots}/{_aiBots.Count} AI bots");
    }

    private void UpdateAIBots()
    {
        // Проверяем, изменился ли список персонажей
        var currentCharacterCount = FindObjectsOfType<CharacterAbstract>().Length;
        if (currentCharacterCount != _allCharacters.Count)
        {
            RefreshCharacterLists();
        }

        // Обновляем информацию о персонажах для всех ботов
        foreach (var bot in _aiBots)
        {
            if (bot != null)
            {
                try
                {
                    bot.UpdateCharactersList(_allCharacters);
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[AIManager] Error updating bot {bot.name}: {ex}");
                }
            }
        }

        // Уведомляем другие системы об обновлении
        OnCharactersUpdated?.Invoke(_allCharacters);

        if (_enableGlobalDebug)
        {
            Debug.Log($"[AIManager] Updated {_aiBots.Count} bots with {_allCharacters.Count} characters");
        }
    }

    #endregion

    #region Inspector Integration

    [ContextMenu("Refresh Character Lists")]
    private void InspectorRefreshLists()
    {
        RefreshCharacterLists();
        Debug.Log("[AIManager] Character lists refreshed from inspector");
    }

    [ContextMenu("Reinitialize All Bots")]
    private void InspectorReinitializeBots()
    {
        RefreshCharacterLists();
        InitializeAllBots();
        Debug.Log("[AIManager] All bots reinitialized from inspector");
    }

    [ContextMenu("Print AI Statistics")]
    private void InspectorPrintStats()
    {
        var stats = GetAIStatistics();
        var report = "[AIManager] Statistics:\n";
        
        foreach (var kvp in stats)
        {
            report += $"  {kvp.Key}: {kvp.Value}\n";
        }
        
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