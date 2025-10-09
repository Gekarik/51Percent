using UnityEngine;

[System.Serializable]
public class BotPersonality
{
    [Range(0f, 1f)] public float aggressiveness = 0.5f;
    [Range(0f, 1f)] public float greediness = 0.5f;      // Желание собирать монетки
    [Range(0f, 1f)] public float riskTolerance = 0.5f;   // Готовность рисковать
    [Range(0f, 1f)] public float territorialness = 0.5f;  // Желание защищать территорию
    [Range(0f, 1f)] public float reactiveness = 0.5f;     // Скорость реакции на опасность
    
    public static BotPersonality CreateFromType(BotBehaviorType type)
    {
        var personality = new BotPersonality();
        
        switch (type)
        {
            case BotBehaviorType.Passive:
                personality.aggressiveness = 0.2f;
                personality.greediness = 0.6f;
                personality.riskTolerance = 0.3f;
                personality.territorialness = 0.4f;
                personality.reactiveness = 0.7f;
                break;
                
            case BotBehaviorType.Aggressive:
                personality.aggressiveness = 0.9f;
                personality.greediness = 0.3f;
                personality.riskTolerance = 0.8f;
                personality.territorialness = 0.6f;
                personality.reactiveness = 0.5f;
                break;
                
            case BotBehaviorType.Collector:
                personality.aggressiveness = 0.3f;
                personality.greediness = 0.95f;
                personality.riskTolerance = 0.4f;
                personality.territorialness = 0.3f;
                personality.reactiveness = 0.6f;
                break;
                
            case BotBehaviorType.Defender:
                personality.aggressiveness = 0.5f;
                personality.greediness = 0.4f;
                personality.riskTolerance = 0.2f;
                personality.territorialness = 0.9f;
                personality.reactiveness = 0.8f;
                break;
                
            case BotBehaviorType.Hunter:
                personality.aggressiveness = 0.95f;
                personality.greediness = 0.2f;
                personality.riskTolerance = 0.9f;
                personality.territorialness = 0.4f;
                personality.reactiveness = 0.7f;
                break;
                
            case BotBehaviorType.Balanced:
            default:
                // Уже инициализированы как 0.5f
                break;
        }
        
        return personality;
    }
}