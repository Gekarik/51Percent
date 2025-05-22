using System;
using System.Collections.Generic;
using UnityEngine;

public class Conquester : MonoBehaviour
{
    public Action<ICharacter, ICharacter> TrailInterrupted;

    private HexGrid _grid;
    private HashSet<Hex> _trail; // Используем HashSet для избежания дублирования
    private HashSet<Hex> _fixed;
    private ICharacter _player;

    private void Awake()
    {
        _player = GetComponent<ICharacter>();
        _trail = new HashSet<Hex>();
        _fixed = new HashSet<Hex>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out Hex hex))
        {
            if (hex.State == HexState.PartOfTrail && _player != hex.Owner)
            {
                TrailInterrupted?.Invoke(hex.Owner, _player);
            }
            else
            {
                AddHexToTrail(hex);
            }
        }
    }

    private void AddHexToTrail(Hex hex)
    {
        if (!_trail.Contains(hex))
        {
            _trail.Add(hex);
            hex.SetState(HexState.PartOfTrail);
        }
    }

    public void GetStartTerritory(Hex startHex)
    {
        if (_fixed.Count == 0 && _player.State == CharacterState.Alive)
        {
            Debug.Log(startHex.Coord.ToString());

            _fixed.UnionWith(BFS(startHex, 2)); // Например, радиус BFS = 2
            _fixed.Add(startHex);

            foreach (Hex hex in _fixed)
            {
                hex.SetOwner(_player);
            }
        }
    }

    private HashSet<Hex> BFS(Hex startHex, int radius)
    {
        var visited = new HashSet<Hex>();
        var queue = new Queue<(Hex hex, int depth)>();
        queue.Enqueue((startHex, 0));
        visited.Add(startHex);

        while (queue.Count > 0)
        {
            var (currentHex, depth) = queue.Dequeue();

            if (depth < radius)
            {
                foreach (Hex neighbor in _grid.GetNeighbors(currentHex.Coord))
                {
                    if (!visited.Contains(neighbor) && neighbor.IsPassable)
                    {
                        visited.Add(neighbor);
                        queue.Enqueue((neighbor, depth + 1));
                    }
                }
            }
        }

        return visited;
    }

    private void FixTerritory()
    {
        foreach (Hex hex in _trail)
        {
            _fixed.Add(hex);
            hex.SetOwner(_player);
            hex.SetState(HexState.Busy);
        }

        _trail.Clear();
    }

    private void Reset()
    {
        _trail.Clear();
        _fixed.Clear();
    }

    internal void Init(HexGrid hexGrid)
    {
        _grid = hexGrid ?? throw new ArgumentNullException(nameof(hexGrid));
    }
}
