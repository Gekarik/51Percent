using UnityEngine;
using AI.Core;

namespace AI.Components
{
    /// <summary>
    /// Компонент для отладочного отображения AI информации
    /// Выделен из AIBrain для разделения ответственности
    /// </summary>
    public class AIDebugRenderer : MonoBehaviour
    {
        [Header("Debug Display Settings")]
        [SerializeField] private bool _showDebugGUI = false;
        [SerializeField] private Vector2 _guiOffset = Vector2.zero;
        [SerializeField] private Vector2 _guiSize = new Vector2(300, 140);
        [SerializeField] private Color _guiBackgroundColor = Color.white;

        private AIContext _context;
        private AIState _currentState;
        private string _currentBehaviorName;
        private BotPersonality _personality;

        /// <summary>
        /// Установить данные для отображения
        /// </summary>
        public void SetDebugData(AIContext context, AIState state, string behaviorName, BotPersonality personality)
        {
            _context = context;
            _currentState = state;
            _currentBehaviorName = behaviorName;
            _personality = personality;
        }

        /// <summary>
        /// Включить/отключить отладочный GUI
        /// </summary>
        public void SetDebugGUIEnabled(bool enabled)
        {
            _showDebugGUI = enabled;
        }

        private void OnGUI()
        {
            if (!_showDebugGUI || _context == null) return;

            // Смещение для нескольких ботов
            var instanceOffset = (GetInstanceID() % 5) * 150;
            var rect = new Rect(10 + _guiOffset.x, 10 + _guiOffset.y + instanceOffset, _guiSize.x, _guiSize.y);
            
            // Цветной фон в зависимости от состояния
            var originalColor = GUI.backgroundColor;
            GUI.backgroundColor = GetStateColor(_currentState);
            
            GUI.Box(rect, $"{name} AI Debug");
            GUI.backgroundColor = originalColor;
            
            // Информационные строки
            var labelRect = new Rect(rect.x + 5, rect.y + 20, rect.width - 10, 20);
            
            GUI.Label(labelRect, $"State: {_currentState}");
            labelRect.y += 20;
            
            GUI.Label(labelRect, $"Behavior: {_currentBehaviorName ?? "None"}");
            labelRect.y += 20;
            
            GUI.Label(labelRect, $"Personality: {_personality}");
            labelRect.y += 20;
            
            if (_context != null)
            {
                GUI.Label(labelRect, $"Territory: {_context.GetTerritoryPercentage():P1}");
                labelRect.y += 20;
                
                GUI.Label(labelRect, $"Enemies nearby: {_context.GetNearbyEnemies().Count}");
                labelRect.y += 20;
                
                var threatLevel = _context.Blackboard.GetFloat("threat_level");
                GUI.Label(labelRect, $"Threat Level: {threatLevel:F2}");
            }
        }

        private void OnDrawGizmos()
        {
            if (_context == null) return;

            // Рисуем радиус обнаружения
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCircle(transform.position, 6f); // Detection radius

            // Рисуем угрозы красным
            var enemies = _context.GetNearbyEnemies();
            Gizmos.color = Color.red;
            foreach (var enemy in enemies)
            {
                if (enemy?.transform != null)
                {
                    Gizmos.DrawLine(transform.position, enemy.transform.position);
                    Gizmos.DrawWireSphere(enemy.transform.position, 1f);
                }
            }

            // Рисуем свою территорию зелёным
            var ownedHexes = _context.GetOwnedHexes();
            Gizmos.color = Color.green;
            foreach (var hex in ownedHexes)
            {
                if (hex?.transform != null)
                {
                    Gizmos.DrawWireCube(hex.transform.position, Vector3.one * 0.5f);
                }
            }
        }

        private Color GetStateColor(AIState state)
        {
            return state switch
            {
                AIState.Initializing => Color.gray,
                AIState.Thinking => Color.yellow,
                AIState.Acting => Color.green,
                AIState.Waiting => Color.blue,
                AIState.Dead => Color.red,
                _ => Color.white
            };
        }

        #region Inspector Tools

        [ContextMenu("Toggle Debug GUI")]
        private void ToggleDebugGUI()
        {
            _showDebugGUI = !_showDebugGUI;
            Debug.Log($"[AIDebugRenderer] Debug GUI {(_showDebugGUI ? "enabled" : "disabled")} for {name}");
        }

        [ContextMenu("Print Context Info")]
        private void PrintContextInfo()
        {
            if (_context != null)
            {
                Debug.Log(_context.GetDebugInfo());
            }
            else
            {
                Debug.Log($"[AIDebugRenderer] {name} - Context not initialized");
            }
        }

        [ContextMenu("Reset GUI Position")]
        private void ResetGUIPosition()
        {
            _guiOffset = Vector2.zero;
            Debug.Log("[AIDebugRenderer] GUI position reset");
        }

        #endregion
    }
}