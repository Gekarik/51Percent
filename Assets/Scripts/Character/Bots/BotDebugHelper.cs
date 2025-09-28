using UnityEngine;

/// <summary>
/// Помогает диагностировать проблемы с ботами
/// </summary>
public class BotDebugHelper : MonoBehaviour
{
    [SerializeField] private bool _autoFindBots = true;
    [SerializeField] private float _checkInterval = 2f;
    
    private float _lastCheck;
    
    private void Update()
    {
        if (_autoFindBots && Time.time - _lastCheck > _checkInterval)
        {
            _lastCheck = Time.time;
            CheckAllBots();
        }
    }
    
    [ContextMenu("Check All Bots")]
    public void CheckAllBots()
    {
        var adaptiveBots = FindObjectsOfType<AdaptiveBot>();
        var enemies = FindObjectsOfType<Enemy>();
        
        Debug.Log($"=== BOT DIAGNOSTIC ===");
        Debug.Log($"Found {adaptiveBots.Length} AdaptiveBots and {enemies.Length} old Enemies");
        
        foreach (var bot in adaptiveBots)
        {
            CheckBot(bot);
        }
        
        foreach (var enemy in enemies)
        {
            CheckEnemy(enemy);
        }
    }
    
    private void CheckBot(AdaptiveBot bot)
    {
        var controller = bot.GetComponent<AdaptiveBotController>();
        var pathProvider = bot.GetComponent<AdaptivePathProvider>();
        var mover = bot.GetComponent<Mover>();
        var conquester = bot.GetComponent<Conquester>();
        
        Debug.Log($"Bot {bot.name}:");
        Debug.Log($"  - State: {(controller != null ? controller.CurrentState.ToString() : "NO CONTROLLER")}");
        Debug.Log($"  - Owned hexes: {(conquester != null ? conquester.FixedHexes.Count : 0)}");
        Debug.Log($"  - PathProvider: {(pathProvider != null ? "OK" : "MISSING")}");
        Debug.Log($"  - Mover: {(mover != null ? "OK" : "MISSING")}");
        Debug.Log($"  - Is moving: {(pathProvider != null ? !pathProvider.IsDone : false)}");
        
        if (pathProvider != null)
        {
            var target = pathProvider.GetCurrentTarget();
            var distance = Vector3.Distance(bot.transform.position, target);
            Debug.Log($"  - Target distance: {distance:F2}");
        }
    }
    
    private void CheckEnemy(Enemy enemy)
    {
        var conquester = enemy.GetComponent<Conquester>();
        Debug.Log($"Old Enemy {enemy.name}: owned hexes = {(conquester != null ? conquester.FixedHexes.Count : 0)}");
    }
}