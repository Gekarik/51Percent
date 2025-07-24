using UnityEngine;

[RequireComponent(typeof(EnemyAIController))]
public class Enemy : CharacterAbstract
{
    private EnemyAIController _ai;

    private void Awake()
    {
        _ai = GetComponent<EnemyAIController>();
        BaseInit();
    }

    public void InitAI(IHexGridProvider grid)
    {
        _ai.Init(grid);
    }
}
