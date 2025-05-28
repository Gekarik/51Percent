using System.Collections.Generic;
// ��������� ������� � ��������� (���� ������)
public interface IHexEnvironment
{
    /// <summary>��� ����� �� �����</summary>
    IReadOnlyList<Hex> AllHexes { get; }

    /// <summary>����� ��������� ������� ��� ���������� �����</summary>
    IEnumerable<Hex> GetNeighbors(Hex hex);
}
