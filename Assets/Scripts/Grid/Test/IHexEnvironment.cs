using System.Collections.Generic;
// Интерфейс доступа к окружению (сеть гексов)
public interface IHexEnvironment
{
    /// <summary>Все гексы на карте</summary>
    IReadOnlyList<Hex> AllHexes { get; }

    /// <summary>Метод получения соседей для указанного гекса</summary>
    IEnumerable<Hex> GetNeighbors(Hex hex);
}
