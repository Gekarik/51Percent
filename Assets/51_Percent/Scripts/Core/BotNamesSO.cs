using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "51_Percent/Bot Names", fileName = "BotNames")]
public class BotNamesSO : ScriptableObject
{
    [SerializeField] private string[] _names;

    private readonly List<string> _available = new();

    private void OnEnable() => Refill();

    public string GetNext()
    {
        if (_names is not { Length: > 0 })
            return "Bot";

        if (_available.Count == 0)
            Refill();

        int index = Random.Range(0, _available.Count);
        string name = _available[index];
        _available.RemoveAt(index);
        return name;
    }

    private void Refill()
    {
        _available.Clear();
        _available.AddRange(_names);
    }
}
