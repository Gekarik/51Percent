using System;
using UnityEngine;

public abstract class CharacterSpawner<T> : MonoBehaviour where T : MonoBehaviour, ICharacter
{
    [SerializeField] private T _prefab;
    [SerializeField] protected HexGrid _grid; //лучше под интерфейсом
    [SerializeField] private TerritoryManager _territoryManager;

    KillManager _killManager;

    private void Awake()
    {
        _killManager = new KillManager();
    }

    private void Start()
    {
        if (_grid == null)
            throw new InvalidOperationException($"HexGrid is empty");
    }

    protected T SpawnSingleCharacter()
    {
        var startHex = _grid.GetRandomHex();

        var character = Instantiate(_prefab, startHex.transform.position, Quaternion.identity);
        _territoryManager.GetStartTerritory(character, startHex);
        character.InitConquester(_grid);
        character.Conquester.TrailInterrupted += _killManager.OnTrailInterrupted;

        return character;
    }
}
