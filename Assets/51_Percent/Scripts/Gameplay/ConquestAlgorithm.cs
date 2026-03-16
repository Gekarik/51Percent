using System.Collections.Generic;

public class ConquestAlgorithm
{
    private const int MaxNeighborCount = 6;

    // Рабочие коллекции — переиспользуются между вызовами, без аллокаций
    private readonly HashSet<IHex> _barrier = new HashSet<IHex>();
    private readonly Queue<IHex> _floodQueue = new Queue<IHex>();
    private readonly HashSet<IHex> _visited = new HashSet<IHex>();

    // Кеш соседей и граничных гексов — строится один раз
    private Dictionary<IHex, IHex[]> _neighborCache;
    private List<IHex> _borderHexes;

    public void ComputeCapturedArea(
        IReadOnlyCollection<IHex> fixedHexes,
        IReadOnlyCollection<IHex> trailHexes,
        IHexGridProvider hexGridProvider,
        List<IHex> output)
    {
        EnsureNeighborCache(hexGridProvider);

        _barrier.Clear();
        foreach (var hex in fixedHexes)
            _barrier.Add(hex);
        foreach (var hex in trailHexes)
            _barrier.Add(hex);

        _visited.Clear();
        _floodQueue.Clear();

        // Засеваем с граничных гексов, которые не в барьере
        foreach (var hex in _borderHexes)
        {
            if (!_barrier.Contains(hex))
            {
                _floodQueue.Enqueue(hex);
                _visited.Add(hex);
            }
        }

        // BFS flood-fill от границ
        while (_floodQueue.Count > 0)
        {
            IHex current = _floodQueue.Dequeue();
            IHex[] neighbors = _neighborCache[current];

            for (int i = 0; i < neighbors.Length; i++)
            {
                IHex neighbor = neighbors[i];

                if (_barrier.Contains(neighbor) || _visited.Contains(neighbor))
                    continue;

                _visited.Add(neighbor);
                _floodQueue.Enqueue(neighbor);
            }
        }

        // Всё, что не достигнуто flood-fill и не в барьере — захвачено
        output.Clear();

        IReadOnlyList<IHex> allHexes = hexGridProvider.AllHexes;
        for (int i = 0; i < allHexes.Count; i++)
        {
            IHex hex = allHexes[i];
            if (!_barrier.Contains(hex) && !_visited.Contains(hex))
                output.Add(hex);
        }

        foreach (var hex in trailHexes)
            output.Add(hex);
    }

    private void EnsureNeighborCache(IHexGridProvider grid)
    {
        if (_neighborCache != null)
            return;

        IReadOnlyList<IHex> allHexes = grid.AllHexes;
        _neighborCache = new Dictionary<IHex, IHex[]>(allHexes.Count);
        _borderHexes = new List<IHex>();

        // Временный список для сборки соседей каждого гекса
        var tempNeighbors = new List<IHex>(MaxNeighborCount);

        for (int i = 0; i < allHexes.Count; i++)
        {
            IHex hex = allHexes[i];
            tempNeighbors.Clear();

            foreach (var neighbor in grid.GetNeighbors(hex))
                tempNeighbors.Add(neighbor);

            IHex[] neighbors = tempNeighbors.ToArray();
            _neighborCache[hex] = neighbors;

            if (neighbors.Length < MaxNeighborCount)
                _borderHexes.Add(hex);
        }
    }
}
