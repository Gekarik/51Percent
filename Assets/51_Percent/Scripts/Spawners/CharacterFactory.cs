using UnityEngine;

public class CharacterFactory<T> where T : CharacterBase
{
    private const float SpawnYOffset = 0.1f;

    private readonly T _prefab;
    private readonly CharacterDependencies _dependencies;

    public CharacterFactory(T prefab, CharacterDependencies dependencies)
    {
        _prefab = prefab;
        _dependencies = dependencies;
    }

    public T Create(IHex hex)
    {
        Vector3 spawnPosition = hex.Transform.position + Vector3.up * SpawnYOffset;
        var character = Object.Instantiate(_prefab, spawnPosition, Quaternion.identity);
        character.Init(_dependencies.ColorService, _dependencies.Territory, _dependencies.Grid,
            _dependencies.MatchState);
        _dependencies.Territory.GetStartTerritory(character, hex);
        return character;
    }
}
