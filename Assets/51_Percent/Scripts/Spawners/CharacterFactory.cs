using UnityEngine;

public class CharacterFactory<T> where T : MonoBehaviour, ICharacter
{
    private const float SpawnYOffset = 0.1f;

    private readonly T _prefab;
    private readonly ColorService _colorService;
    private readonly TerritoryManager _territoryManager;
    private readonly IHexGridProvider _grid;
    private readonly KillManager _killManager;

    public CharacterFactory(T prefab, ColorService colorService, TerritoryManager territoryManager,
        IHexGridProvider grid, KillManager killManager)
    {
        _prefab = prefab;
        _colorService = colorService;
        _territoryManager = territoryManager;
        _grid = grid;
        _killManager = killManager;
    }

    public T Create(IHex hex)
    {
        Vector3 spawnPosition = hex.Transform.position + Vector3.up * SpawnYOffset;
        var character = Object.Instantiate(_prefab, spawnPosition, Quaternion.identity);
        character.Init(_colorService, _territoryManager, _grid, _killManager);
        _territoryManager.GetStartTerritory(character, hex);
        return character;
    }
}
