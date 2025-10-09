using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BotManager : MonoBehaviour
{
    [System.Serializable]
    public class BotConfig
    {
        public GameObject botPrefab;
        public BotBehaviorType behaviorType;
        [Range(0.1f, 1f)] public float spawnProbability = 0.5f;
        public string botName = "Bot";
    }

    [Header("Bot Settings")]
    [SerializeField] private List<BotConfig> botConfigs = new List<BotConfig>();
    [SerializeField] private int maxBots = 10;
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private float minSpawnDistance = 10f;
    [SerializeField] private bool autoSpawn = true;
    [SerializeField] private bool balanceTeams = true;

    [Header("Difficulty Settings")]
    [SerializeField, Range(0f, 1f)] private float difficultyLevel = 0.5f;
    [SerializeField] private bool adaptiveDifficulty = true;
    [SerializeField] private float difficultyUpdateInterval = 30f;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
    [SerializeField] private List<SmartBot> activeBots = new List<SmartBot>();

    private IHexGridProvider _gridProvider;
    private Transform _playerTransform;
    private float _lastSpawnTime;
    private float _lastDifficultyUpdate;
    private int _botsSpawned = 0;
    
    // Статистика для адаптивной сложности
    private float _playerSuccessRate = 0.5f;
    private int _playerKills = 0;
    private int _playerDeaths = 0;

    private void Awake()
    {
        // Находим игровую сетку
        _gridProvider = FindObjectOfType<HexGrid>();
        
        // Находим игрока
        var player = FindObjectOfType<Player>();
        if (player != null)
        {
            _playerTransform = player.transform;
        }
    }

    private void Start()
    {
        if (autoSpawn)
        {
            StartCoroutine(BotSpawnLoop());
        }

        if (adaptiveDifficulty)
        {
            StartCoroutine(DifficultyAdjustmentLoop());
        }
    }

    private IEnumerator BotSpawnLoop()
    {
        yield return new WaitForSeconds(2f); // Начальная задержка

        while (true)
        {
            if (Time.time - _lastSpawnTime >= spawnInterval)
            {
                TrySpawnBot();
                _lastSpawnTime = Time.time;
            }

            // Удаляем мертвых ботов из списка
            activeBots.RemoveAll(bot => bot == null || bot.State == CharacterState.Died);

            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator DifficultyAdjustmentLoop()
    {
        while (adaptiveDifficulty)
        {
            yield return new WaitForSeconds(difficultyUpdateInterval);
            UpdateDifficulty();
        }
    }

    private void TrySpawnBot()
    {
        // Проверяем лимит ботов
        if (activeBots.Count >= maxBots)
        {
            return;
        }

        // Выбираем конфигурацию бота на основе вероятностей
        BotConfig config = SelectBotConfig();
        if (config == null || config.botPrefab == null)
        {
            Debug.LogWarning("No valid bot configuration found!");
            return;
        }

        // Находим точку спавна
        Vector3 spawnPosition = FindSpawnPosition();
        if (spawnPosition == Vector3.zero)
        {
            Debug.LogWarning("Could not find valid spawn position!");
            return;
        }

        // Спавним бота
        SpawnBot(config, spawnPosition);
    }

    private BotConfig SelectBotConfig()
    {
        if (botConfigs.Count == 0)
            return null;

        // Выбираем конфигурацию с учетом сложности
        List<BotConfig> availableConfigs = new List<BotConfig>();
        
        foreach (var config in botConfigs)
        {
            float adjustedProbability = config.spawnProbability * (1f + difficultyLevel);
            
            // Более агрессивные боты спавнятся чаще при высокой сложности
            if (config.behaviorType == BotBehaviorType.Aggressive || 
                config.behaviorType == BotBehaviorType.Hunter)
            {
                adjustedProbability *= (1f + difficultyLevel * 0.5f);
            }
            
            if (Random.value <= adjustedProbability)
            {
                availableConfigs.Add(config);
            }
        }

        if (availableConfigs.Count == 0)
        {
            // Если ничего не подошло, берем случайный
            return botConfigs[Random.Range(0, botConfigs.Count)];
        }

        return availableConfigs[Random.Range(0, availableConfigs.Count)];
    }

    private Vector3 FindSpawnPosition()
    {
        // Пробуем найти подходящую позицию для спавна
        int maxAttempts = 20;
        
        for (int i = 0; i < maxAttempts; i++)
        {
            // Выбираем случайный hex на краю карты или в свободной зоне
            var availableHexes = _gridProvider.AllHexes
                .Where(h => h.State == HexState.Empty /*|| h.State == HexState.Default*/)
                .ToList();

            if (availableHexes.Count == 0)
                continue;

            var randomHex = availableHexes[Random.Range(0, availableHexes.Count)];
            Vector3 position = randomHex.transform.position;

            // Проверяем расстояние от игрока
            if (_playerTransform != null)
            {
                float distanceToPlayer = Vector3.Distance(position, _playerTransform.position);
                if (distanceToPlayer < minSpawnDistance)
                    continue;
            }

            // Проверяем расстояние от других ботов
            bool tooCloseToOtherBots = false;
            foreach (var bot in activeBots)
            {
                if (bot == null) continue;
                
                float distanceToBot = Vector3.Distance(position, bot.transform.position);
                if (distanceToBot < minSpawnDistance * 0.5f)
                {
                    tooCloseToOtherBots = true;
                    break;
                }
            }

            if (!tooCloseToOtherBots)
            {
                return position;
            }
        }

        // Если не нашли идеальную позицию, берем любую доступную
        var fallbackHexes = _gridProvider.AllHexes
            .Where(h => h.State == HexState.Empty /*|| h.State == HexState.Default*/)
            .ToList();
            
        if (fallbackHexes.Count > 0)
        {
            return fallbackHexes[Random.Range(0, fallbackHexes.Count)].transform.position;
        }

        return Vector3.zero;
    }

    private void SpawnBot(BotConfig config, Vector3 position)
    {
        GameObject botObject = Instantiate(config.botPrefab, position, Quaternion.identity);
        
        // Настраиваем имя бота
        _botsSpawned++;
        botObject.name = $"{config.botName}_{_botsSpawned}";

        // Получаем компоненты бота
        SmartBot smartBot = botObject.GetComponent<SmartBot>();
        SmartBotController controller = botObject.GetComponent<SmartBotController>();

        if (smartBot == null)
        {
            // Если это старый бот, пробуем найти Enemy
            Enemy oldBot = botObject.GetComponent<Enemy>();
            if (oldBot != null)
            {
                oldBot.InitAI(_gridProvider);
            }
            else
            {
                Debug.LogError($"Bot prefab {config.botPrefab.name} doesn't have SmartBot or Enemy component!");
                Destroy(botObject);
                return;
            }
        }
        else
        {
            // Настраиваем умного бота
            smartBot.InitAI(_gridProvider);
            smartBot.SetBehaviorType(config.behaviorType);

            // Настраиваем параметры на основе сложности
            if (controller != null)
            {
                AdjustBotDifficulty(controller);
            }

            activeBots.Add(smartBot);
        }

        if (showDebugInfo)
        {
            Debug.Log($"Spawned {config.behaviorType} bot at {position}");
        }
    }

    private void AdjustBotDifficulty(SmartBotController controller)
    {
        // Настраиваем параметры бота на основе текущей сложности
        // Это можно расширить, добавив методы в SmartBotController для изменения параметров
        
        // Пример: увеличиваем радиусы обзора при высокой сложности
        // controller.SetSightRange(15f + difficultyLevel * 10f);
        // controller.SetDecisionInterval(0.5f - difficultyLevel * 0.2f);
    }

    private void UpdateDifficulty()
    {
        if (!adaptiveDifficulty)
            return;

        // Рассчитываем успешность игрока
        if (_playerKills + _playerDeaths > 0)
        {
            _playerSuccessRate = (float)_playerKills / (_playerKills + _playerDeaths);
        }

        // Корректируем сложность
        if (_playerSuccessRate > 0.7f)
        {
            // Игрок слишком успешен - увеличиваем сложность
            difficultyLevel = Mathf.Min(1f, difficultyLevel + 0.1f);
        }
        else if (_playerSuccessRate < 0.3f)
        {
            // Игрок испытывает трудности - уменьшаем сложность
            difficultyLevel = Mathf.Max(0f, difficultyLevel - 0.1f);
        }

        // Обновляем параметры активных ботов
        foreach (var bot in activeBots)
        {
            if (bot != null)
            {
                var controller = bot.GetComponent<SmartBotController>();
                if (controller != null)
                {
                    AdjustBotDifficulty(controller);
                }
            }
        }

        if (showDebugInfo)
        {
            Debug.Log($"Difficulty adjusted to {difficultyLevel:F2} (Player success rate: {_playerSuccessRate:F2})");
        }
    }

    #region Public Methods

    public void SpawnBotManually(BotBehaviorType behaviorType, Vector3 position)
    {
        var config = botConfigs.FirstOrDefault(c => c.behaviorType == behaviorType);
        if (config != null)
        {
            SpawnBot(config, position);
        }
    }

    public void RemoveAllBots()
    {
        foreach (var bot in activeBots)
        {
            if (bot != null)
            {
                Destroy(bot.gameObject);
            }
        }
        activeBots.Clear();
    }

    public void SetMaxBots(int max)
    {
        maxBots = Mathf.Max(1, max);
    }

    public void SetDifficulty(float difficulty)
    {
        difficultyLevel = Mathf.Clamp01(difficulty);
    }

    public void RegisterPlayerKill()
    {
        _playerKills++;
    }

    public void RegisterPlayerDeath()
    {
        _playerDeaths++;
    }

    public List<SmartBot> GetActiveBots()
    {
        return activeBots.Where(b => b != null).ToList();
    }

    #endregion

    #region Debug

    private void OnGUI()
    {
        if (!showDebugInfo)
            return;

        GUI.Box(new Rect(10, 10, 250, 150), "Bot Manager Debug");
        
        int y = 35;
        GUI.Label(new Rect(15, y, 240, 20), $"Active Bots: {activeBots.Count}/{maxBots}");
        y += 20;
        GUI.Label(new Rect(15, y, 240, 20), $"Difficulty: {difficultyLevel:F2}");
        y += 20;
        GUI.Label(new Rect(15, y, 240, 20), $"Player Success Rate: {_playerSuccessRate:F2}");
        y += 20;
        GUI.Label(new Rect(15, y, 240, 20), $"Kills: {_playerKills} Deaths: {_playerDeaths}");
        y += 20;
        
        // Показываем типы активных ботов
        var botTypes = activeBots
            .Where(b => b != null)
            .GroupBy(b => b.GetComponent<SmartBotController>()?.ToString())
            .Select(g => $"{g.Key}: {g.Count()}")
            .ToList();
            
        GUI.Label(new Rect(15, y, 240, 20), "Bot Types:");
        y += 20;
        foreach (var type in botTypes)
        {
            GUI.Label(new Rect(15, y, 240, 20), type);
            y += 20;
        }
    }

    #endregion
}