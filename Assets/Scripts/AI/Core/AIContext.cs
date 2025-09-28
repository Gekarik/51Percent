using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AI.Core
{
    /// <summary>
    /// Контекст для передачи данных между компонентами ИИ
    /// Содержит всю необходимую информацию о состоянии игры
    /// </summary>
    public class AIContext
    {
        // Основные компоненты
        public ICharacter Character { get; private set; }
        public IHexGridProvider Grid { get; private set; }
        public Blackboard Blackboard { get; private set; }
        
        // Настройки ИИ
        public BotPersonality Personality { get; private set; }
        public float AggressionLevel { get; set; } = 0.5f;
        public float CautiousLevel { get; set; } = 0.5f;
        
        // Кэшированные данные (обновляются сенсорами)
        private List<ICharacter> _allEnemies;
        private List<ICharacter> _nearbyEnemies;
        private ICharacter _nearestThreat;
        private float _lastEnemyUpdate = 0f;
        private const float ENEMY_UPDATE_INTERVAL = 0.5f; // Обновляем список врагов каждые 0.5 сек

        public AIContext(ICharacter character, IHexGridProvider grid, BotPersonality personality)
        {
            Character = character ?? throw new ArgumentNullException(nameof(character));
            Grid = grid ?? throw new ArgumentNullException(nameof(grid));
            Personality = personality;
            Blackboard = new Blackboard();
            
            _allEnemies = new List<ICharacter>();
            _nearbyEnemies = new List<ICharacter>();
            
            InitializeBlackboard();
        }

        private void InitializeBlackboard()
        {
            // Устанавливаем начальные значения
            Blackboard.Set(\"personality\", Personality.ToString());
            Blackboard.Set(\"aggression_level\", AggressionLevel);
            Blackboard.Set(\"cautious_level\", CautiousLevel);
            Blackboard.Set(\"territory_percentage\", 0f);
            Blackboard.Set(\"threat_level\", 0f);
            Blackboard.Set(\"last_decision_time\", Time.time);
        }

        /// <summary>
        /// Обновить список всех врагов (вызывается извне)
        /// </summary>
        public void UpdateEnemies(IEnumerable<ICharacter> allCharacters)
        {
            if (Time.time - _lastEnemyUpdate < ENEMY_UPDATE_INTERVAL)
                return;

            _allEnemies.Clear();
            _allEnemies.AddRange(allCharacters.Where(c => c != Character && c.State == CharacterState.Alive));
            
            UpdateNearbyEnemies();
            _lastEnemyUpdate = Time.time;
        }

        private void UpdateNearbyEnemies()
        {
            const float NEARBY_DISTANCE = 10f; // Радиус \"рядом\"
            var myPosition = Character.transform.position;
            
            _nearbyEnemies.Clear();
            _nearbyEnemies.AddRange(_allEnemies.Where(enemy => 
                Vector3.Distance(enemy.transform.position, myPosition) <= NEARBY_DISTANCE));

            // Обновляем ближайшую угрозу
            _nearestThreat = _nearbyEnemies
                .OrderBy(enemy => Vector3.Distance(enemy.transform.position, myPosition))
                .FirstOrDefault();

            // Сохраняем в Blackboard
            Blackboard.Set(\"nearby_enemies_count\", _nearbyEnemies.Count);
            Blackboard.Set(\"nearest_threat\", _nearestThreat);
            
            if (_nearestThreat != null)
            {
                var distance = Vector3.Distance(_nearestThreat.transform.position, myPosition);
                Blackboard.Set(\"nearest_threat_distance\", distance);
            }
        }

        /// <summary>
        /// Получить всех врагов
        /// </summary>
        public IReadOnlyList<ICharacter> GetAllEnemies()
        {
            return _allEnemies.AsReadOnly();
        }

        /// <summary>
        /// Получить ближайших врагов
        /// </summary>
        public IReadOnlyList<ICharacter> GetNearbyEnemies()
        {
            return _nearbyEnemies.AsReadOnly();
        }

        /// <summary>
        /// Получить ближайшую угрозу
        /// </summary>
        public ICharacter GetNearestThreat()
        {
            return _nearestThreat;
        }

        /// <summary>
        /// Проверить, находится ли персонаж в опасности
        /// </summary>
        public bool IsInDanger()
        {
            var threatLevel = Blackboard.GetFloat(\"threat_level\", 0f);
            return threatLevel > 0.7f;
        }

        /// <summary>
        /// Проверить, есть ли возможности для атаки
        /// </summary>
        public bool HasAttackOpportunity()
        {
            return Blackboard.HasKey(\"vulnerable_trail\") || 
                   Blackboard.HasKey(\"weak_enemy\");
        }

        /// <summary>
        /// Получить процент контролируемой территории
        /// </summary>
        public float GetTerritoryPercentage()
        {
            var ownedCount = Character.Conquester.FixedHexes.Count;
            var totalCount = Grid.AllHexes.Count;
            var percentage = totalCount > 0 ? (float)ownedCount / totalCount : 0f;
            
            Blackboard.Set(\"territory_percentage\", percentage);
            return percentage;
        }

        /// <summary>
        /// Получить безопасность текущей позиции (0-1)
        /// </summary>
        public float GetPositionSafety()
        {
            var nearbyCount = _nearbyEnemies.Count;
            var myTerritorySize = Character.Conquester.FixedHexes.Count;
            
            // Чем больше территории и меньше врагов рядом - тем безопаснее
            var territorySafety = Mathf.Clamp01(myTerritorySize / 20f); // Предполагаем 20 как \"большая территория\"
            var enemySafety = Mathf.Clamp01(1f - (nearbyCount / 3f)); // 3+ врага = опасно
            
            var safety = (territorySafety + enemySafety) / 2f;
            Blackboard.Set(\"position_safety\", safety);
            return safety;
        }

        /// <summary>
        /// Обновить данные контекста (вызывается периодически)
        /// </summary>
        public void Update()
        {
            // Очищаем устаревшие данные
            Blackboard.CleanupExpiredValues();
            
            // Обновляем базовые метрики
            GetTerritoryPercentage();
            GetPositionSafety();
            
            // Обновляем время последнего обновления
            Blackboard.Set(\"last_update_time\", Time.time);
        }

        /// <summary>
        /// Сброс контекста (например, при смерти)
        /// </summary>
        public void Reset()
        {
            Blackboard.Clear();
            _allEnemies.Clear();
            _nearbyEnemies.Clear();
            _nearestThreat = null;
            InitializeBlackboard();
        }

        /// <summary>
        /// Получить отладочную информацию
        /// </summary>
        public string GetDebugInfo()
        {
            var info = $\"AIContext Debug Info:\\n\";
            info += $\"Character: {Character.name}\\n\";
            info += $\"Personality: {Personality}\\n\";
            info += $\"Territory: {GetTerritoryPercentage():P1}\\n\";
            info += $\"Nearby Enemies: {_nearbyEnemies.Count}\\n\";
            info += $\"Threat Level: {Blackboard.GetFloat(\"threat_level\"):F2}\\n\";
            info += $\"Position Safety: {GetPositionSafety():F2}\\n\";
            info += $\"Blackboard Entries: {Blackboard.Count}\\n\";
            
            return info;
        }
    }
}