using System.Collections.Generic;
using UnityEngine;

public class Enemy : CharacterBase
{
    private EnemyBrain _brain;

    protected override void OnInit()
    {
        _brain = GetComponent<EnemyBrain>();
    }

    public void InitBrain(IHexGridProvider grid, IReadOnlyList<ICharacter> allCharacters,
        ICollectibleRegistry collectibleRegistry, BotPersonalitySettings personality = null, int botIndex = 0)
    {
        if (_brain != null)
            _brain.Init(grid, allCharacters, collectibleRegistry, personality, botIndex);
    }
}
