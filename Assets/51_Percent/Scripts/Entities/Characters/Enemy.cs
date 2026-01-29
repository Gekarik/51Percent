using UnityEngine;

public class Enemy : CharacterBase
{
    private EnemyBrain _brain;

    protected override void OnInit()
    {
        _brain = GetComponent<EnemyBrain>();
    }

    public void InitBrain(IHexGridProvider grid, BotPersonalitySettings personality = null)
    {
        if (_brain != null)
        {
            _brain.Init(grid, personality);
        }
    }
}
