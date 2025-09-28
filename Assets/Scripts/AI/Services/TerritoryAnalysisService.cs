using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AI.Services
{
    /// <summary>
    /// Сервис анализа территории - помогает ботам принимать решения о расширении
    /// Оптимизирован для WebGL без статических данных
    /// </summary>
    public class TerritoryAnalysisService : MonoBehaviour
    {
        [Header("Analysis Settings")]
        [SerializeField] private float _analysisInterval = 2f;
        [SerializeField] private int _maxAnalysisTargets = 20;

        private float _lastAnalysisTime;

        /// <summary>
        /// Найти лучшие области для расширения территории
        /// </summary>
        public List<IHex> FindBestExpansionArea(IReadOnlyCollection<IHex> ownedHexes, 
            IHexGridProvider grid, List<ICharacter> enemies)
        {
            if (ownedHexes == null || ownedHexes.Count == 0) return new List<IHex>();

            // Кэшируем анализ для производительности
            if (Time.time - _lastAnalysisTime < _analysisInterval)
            {
                return new List<IHex>(); // Возвращаем пустой список если слишком рано
            }

            _lastAnalysisTime = Time.time;

            var candidates = grid.AllHexes
                .Where(hex => !ownedHexes.Contains(hex))
                .Where(hex => hex.Owner == null || hex.State != HexState.Busy)
                .Where(hex => IsNearOwnedTerritory(hex, ownedHexes, grid))
                .Take(_maxAnalysisTargets) // Ограничиваем для производительности
                .OrderBy(hex => CalculateExpansionScore(hex, ownedHexes, enemies))
                .Take(5) // Топ 5 кандидатов
                .ToList();

            return candidates;
        }

        /// <summary>
        /// Оценить безопасность текущей территории
        /// </summary>
        public float AnalyzeTerritorialSafety(IReadOnlyCollection<IHex> ownedHexes, 
            List<ICharacter> enemies, ICharacter owner)
        {
            if (ownedHexes == null || ownedHexes.Count == 0) return 0f;

            var borderHexes = GetBorderHexes(ownedHexes);
            if (borderHexes.Count == 0) return 1f;

            var threatenedBorder = 0;
            const float THREAT_RADIUS = 5f;

            foreach (var borderHex in borderHexes)
            {
                foreach (var enemy in enemies)
                {
                    if (enemy == owner || enemy?.transform == null) continue;

                    var distance = Vector3.Distance(borderHex.transform.position, enemy.transform.position);
                    if (distance < THREAT_RADIUS)
                    {
                        threatenedBorder++;
                        break;
                    }
                }
            }

            return 1f - (threatenedBorder / (float)borderHexes.Count);
        }

        #region Private Methods

        private bool IsNearOwnedTerritory(IHex hex, IReadOnlyCollection<IHex> ownedHexes, IHexGridProvider grid)
        {
            // Проверяем, есть ли среди соседей наши гексы
            return grid.GetNeighbors(hex).Any(neighbor => ownedHexes.Contains(neighbor));
        }

        private float CalculateExpansionScore(IHex hex, IReadOnlyCollection<IHex> ownedHexes, List<ICharacter> enemies)
        {
            var score = 0f;

            // Близость к нашей территории (меньше = лучше)
            var distanceToOwned = ownedHexes
                .Min(owned => Vector3.Distance(hex.transform.position, owned.transform.position));
            score += distanceToOwned * 0.3f;

            // Удаленность от врагов (больше = лучше)
            if (enemies != null && enemies.Count > 0)
            {
                var distanceToEnemies = enemies
                    .Where(enemy => enemy?.transform != null)
                    .Min(enemy => Vector3.Distance(hex.transform.position, enemy.transform.position));
                score -= distanceToEnemies * 0.2f;
            }

            return score;
        }

        private List<IHex> GetBorderHexes(IReadOnlyCollection<IHex> ownedHexes)
        {
            // Упрощенный алгоритм поиска границы
            // В реальности можно улучшить
            return ownedHexes.ToList(); // Заглушка
        }

        #endregion

        #region Inspector Tools

        [ContextMenu("Reset Analysis Cache")]
        private void ResetAnalysisCache()
        {
            _lastAnalysisTime = 0f;
            Debug.Log("[TerritoryAnalysisService] Analysis cache reset");
        }

        #endregion
    }
}