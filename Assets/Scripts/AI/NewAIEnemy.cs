using UnityEngine;
using AI.Core;

/// <summary>
/// Новый класс врага с улучшенной AI системой
/// Заменяет старый Enemy.cs
/// </summary>
[RequireComponent(typeof(AIBrain))]
public class NewAIEnemy : CharacterAbstract
{
    [Header("AI Configuration")]
    [SerializeField] private BotPersonality _personality = BotPersonality.Balanced;
    [SerializeField] private bool _enableDebugVisualization = false;
    
    private AIBrain _aiBrain;
    private bool _isInitialized = false;

    private void Awake()
    {
        // Инициализируем базовый класс
        BaseInit();
        
        // Получаем AI компонент
        _aiBrain = GetComponent<AIBrain>();
        if (_aiBrain == null)
        {
            Debug.LogError($"[NewAIEnemy] No AIBrain component found on {name}");
            enabled = false;
            return;
        }
    }

    /// <summary>
    /// Инициализация AI (вызывается извне, например, из спавнера)
    /// </summary>
    public void InitializeAI(IHexGridProvider grid, System.Collections.Generic.List<ICharacter> allCharacters)
    {
        if (_aiBrain == null)
        {
            Debug.LogError($"[NewAIEnemy] Cannot initialize AI - no AIBrain component on {name}");
            return;
        }

        try
        {
            // Инициализируем AI мозг
            _aiBrain.Initialize(grid, allCharacters);
            _isInitialized = true;
            
            Debug.Log($"[NewAIEnemy] {name} AI initialized successfully with personality: {_personality}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[NewAIEnemy] Failed to initialize AI for {name}: {ex}");
            enabled = false;
        }
    }

    /// <summary>
    /// Обновить список персонажей для AI
    /// </summary>
    public void UpdateCharactersList(System.Collections.Generic.List<ICharacter> allCharacters)
    {
        if (_isInitialized && _aiBrain != null)
        {
            _aiBrain.UpdateCharactersList(allCharacters);
        }
    }

    /// <summary>
    /// Получить текущее состояние AI для отладки
    /// </summary>
    public string GetAIStatus()
    {
        if (!_isInitialized || _aiBrain == null) 
            return "AI not initialized";

        var context = _aiBrain.Context;
        if (context == null) 
            return "AI context is null";

        return $"State: {_aiBrain.CurrentState}, Behavior: {_aiBrain.CurrentBehavior?.Name ?? "None"}, Territory: {context.GetTerritoryPercentage():P1}";
    }

    /// <summary>
    /// Остановить AI (например, при смерти)
    /// </summary>
    public new void Die()
    {
        if (_aiBrain != null)
        {
            _aiBrain.Stop();
        }
        
        base.Die();
    }

    /// <summary>
    /// Включить/выключить отладочную визуализацию
    /// </summary>
    public void SetDebugVisualization(bool enabled)
    {
        _enableDebugVisualization = enabled;
    }

    private void OnDrawGizmos()
    {
        if (!_enableDebugVisualization || !_isInitialized || _aiBrain?.Context == null) 
            return;

        DrawAIDebugGizmos();
    }

    private void DrawAIDebugGizmos()
    {
        var context = _aiBrain.Context;
        var myPosition = transform.position;

        // Рисуем территорию
        Gizmos.color = Color.green;
        foreach (var hex in this.Conquester.FixedHexes)
        {
            if (hex?.transform != null)
            {
                Gizmos.DrawWireCube(hex.transform.position, Vector3.one * 0.5f);
            }
        }

        // Рисуем радиус обнаружения врагов
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(myPosition, 10f); // NEARBY_DISTANCE из AIContext

        // Рисуем связи с ближайшими врагами
        Gizmos.color = Color.red;
        foreach (var enemy in context.GetNearbyEnemies())
        {
            if (enemy?.transform != null)
            {
                Gizmos.DrawLine(myPosition, enemy.transform.position);
            }
        }

        // Рисуем текущую цель (если есть)
        var nearestThreat = context.GetNearestThreat();
        if (nearestThreat?.transform != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(nearestThreat.transform.position, 2f);
            Gizmos.DrawLine(myPosition, nearestThreat.transform.position);
        }

        // Показываем уровень угрозы цветом персонажа
        var threatLevel = context.Blackboard.GetFloat("threat_level", 0f);
        Gizmos.color = Color.Lerp(Color.green, Color.red, threatLevel);
        Gizmos.DrawWireSphere(myPosition, 1f);
    }

    #region Inspector Integration

    [ContextMenu("Initialize AI (Test)")]
    private void TestInitializeAI()
    {
        var grid = FindObjectOfType<HexGrid>();
        var characters = new System.Collections.Generic.List<ICharacter>();
        
        // Находим всех персонажей в сцене
        var allCharacterObjects = FindObjectsOfType<CharacterAbstract>();
        foreach (var character in allCharacterObjects)
        {
            characters.Add(character);
        }

        if (grid != null)
        {
            InitializeAI(grid, characters);
        }
        else
        {
            Debug.LogWarning("[NewAIEnemy] No HexGrid found in scene for test initialization");
        }
    }

    [ContextMenu("Print AI Status")]
    private void PrintAIStatus()
    {
        Debug.Log($"[NewAIEnemy] {name}: {GetAIStatus()}");
        
        if (_aiBrain?.Context != null)
        {
            Debug.Log(_aiBrain.Context.GetDebugInfo());
        }
    }

    [ContextMenu("Toggle Debug Visualization")]
    private void ToggleDebugVisualization()
    {
        SetDebugVisualization(!_enableDebugVisualization);
        Debug.Log($"[NewAIEnemy] Debug visualization {(_enableDebugVisualization ? "enabled" : "disabled")} for {name}");
    }

    #endregion

    #region Compatibility with existing code

    /// <summary>
    /// Совместимость со старым API - инициализация AI
    /// </summary>
    public void InitAI(IHexGridProvider grid)
    {
        // Для совместимости с существующим кодом
        // Найдём всех персонажей в сцене
        var allCharacters = new System.Collections.Generic.List<ICharacter>();
        var characterObjects = FindObjectsOfType<CharacterAbstract>();
        
        foreach (var character in characterObjects)
        {
            allCharacters.Add(character);
        }

        InitializeAI(grid, allCharacters);
    }

    #endregion
}