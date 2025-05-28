using System.Collections.Generic;
// Интерфейс для хранения текущего состояния захвата
public interface IConquestState
{
    /// <summary>Набор уже захваченных (фиксированных) гексов</summary>
    ISet<Hex> FixedHexes { get; }

    /// <summary>Список гексов текущего трейла (барьерной цепи)</summary>
    IList<Hex> TrailHexes { get; }
}
