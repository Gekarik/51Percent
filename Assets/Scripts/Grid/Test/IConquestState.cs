using System.Collections.Generic;
// ��������� ��� �������� �������� ��������� �������
public interface IConquestState
{
    /// <summary>����� ��� ����������� (�������������) ������</summary>
    ISet<Hex> FixedHexes { get; }

    /// <summary>������ ������ �������� ������ (��������� ����)</summary>
    IList<Hex> TrailHexes { get; }
}
