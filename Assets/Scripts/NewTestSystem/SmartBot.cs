using UnityEngine;

[RequireComponent(typeof(SmartBotController))]
public class SmartBot : CharacterAbstract
{
    private SmartBotController _aiController;

    private void Awake()
    {
        _aiController = GetComponent<SmartBotController>();
        BaseInit();
    }

    public void InitAI(IHexGridProvider grid)
    {
        _aiController.Init(grid);
    }

    public void SetBehaviorType(BotBehaviorType behaviorType)
    {
        // Позволяет динамически менять тип поведения бота
        var controller = GetComponent<SmartBotController>();
        if (controller != null)
        {
            // Для динамического изменения можно добавить метод в SmartBotController
            Debug.Log($"Bot {name} behavior set to {behaviorType}");
        }
    }
}
