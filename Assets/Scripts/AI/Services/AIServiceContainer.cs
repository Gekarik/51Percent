using UnityEngine;

namespace AI.Services
{
    /// <summary>
    /// Контейнер сервисов для AI системы - заменяет статические классы
    /// Оптимизирован для WebGL/Яндекс игр
    /// </summary>
    public class AIServiceContainer : MonoBehaviour
    {
        [Header("AI Services")]
        [SerializeField] private BehaviorUtilsService _behaviorUtils;
        [SerializeField] private PathfindingService _pathfindingService;
        [SerializeField] private TerritoryAnalysisService _territoryAnalysis;

        // Singleton для удобства доступа, но без статических полей данных
        private static AIServiceContainer _instance;
        
        public static AIServiceContainer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<AIServiceContainer>();
                    
                    if (_instance == null)
                    {
                        Debug.LogError("[AIServiceContainer] No AIServiceContainer found in scene!");
                    }
                }
                return _instance;
            }
        }

        // Свойства для доступа к сервисам
        public static BehaviorUtilsService BehaviorUtils => Instance?._behaviorUtils;
        public static PathfindingService Pathfinding => Instance?._pathfindingService;
        public static TerritoryAnalysisService TerritoryAnalysis => Instance?._territoryAnalysis;

        private void Awake()
        {
            // Проверяем единственность экземпляра
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("[AIServiceContainer] Multiple instances detected, destroying duplicate");
                Destroy(gameObject);
                return;
            }

            _instance = this;
            
            // Не делаем DontDestroyOnLoad для WebGL - лучше пересоздавать
            // DontDestroyOnLoad(gameObject);
            
            InitializeServices();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void InitializeServices()
        {
            // Создаём сервисы если они не назначены
            if (_behaviorUtils == null)
            {
                var go = new GameObject("BehaviorUtilsService");
                go.transform.SetParent(transform);
                _behaviorUtils = go.AddComponent<BehaviorUtilsService>();
            }

            if (_pathfindingService == null)
            {
                var go = new GameObject("PathfindingService");
                go.transform.SetParent(transform);
                _pathfindingService = go.AddComponent<PathfindingService>();
            }

            if (_territoryAnalysis == null)
            {
                var go = new GameObject("TerritoryAnalysisService");
                go.transform.SetParent(transform);
                _territoryAnalysis = go.AddComponent<TerritoryAnalysisService>();
            }

            Debug.Log("[AIServiceContainer] AI Services initialized successfully");
        }

        /// <summary>
        /// Проверить доступность всех сервисов
        /// </summary>
        public bool AreServicesReady()
        {
            return _behaviorUtils != null && 
                   _pathfindingService != null && 
                   _territoryAnalysis != null;
        }

        /// <summary>
        /// Получить сервис определённого типа
        /// </summary>
        public T GetService<T>() where T : MonoBehaviour
        {
            if (typeof(T) == typeof(BehaviorUtilsService))
                return _behaviorUtils as T;
            if (typeof(T) == typeof(PathfindingService))
                return _pathfindingService as T;
            if (typeof(T) == typeof(TerritoryAnalysisService))
                return _territoryAnalysis as T;
                
            Debug.LogWarning($"[AIServiceContainer] Service of type {typeof(T)} not found");
            return null;
        }

        #region Inspector Tools

        [ContextMenu("Validate Services")]
        private void ValidateServices()
        {
            var report = "[AIServiceContainer] Service Status:\n";
            report += $"  BehaviorUtils: {(_behaviorUtils != null ? "✓" : "✗")}\n";
            report += $"  Pathfinding: {(_pathfindingService != null ? "✓" : "✗")}\n";
            report += $"  TerritoryAnalysis: {(_territoryAnalysis != null ? "✓" : "✗")}\n";
            
            Debug.Log(report);
        }

        [ContextMenu("Reinitialize Services")]
        private void ReinitializeServices()
        {
            InitializeServices();
            Debug.Log("[AIServiceContainer] Services reinitialized");
        }

        #endregion
    }
}