using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class GridLookupTests : GameplayScenario
{
    private const float InsideCellFactor = 0.45f;
    private const float NearCenterFactor = 0.2f;
    private const int DirectionCount = 12;

    private const int CellSamplingStep = 37;

    [UnityTest]
    public IEnumerator GetHexAt_AtCellCenter_ReturnsThatCell()
    {
        foreach (Component hex in SampledCells())
            Assert.That(HexAt(hex.transform.position), Is.SameAs(hex),
                $"Центр клетки {hex.name} определился как другая клетка.");

        yield break;
    }

    [UnityTest]
    public IEnumerator GetHexAt_NearSlantedEdges_ReturnsOwningCell_NotNeighbour()
    {
        float step = LatticeStep();

        foreach (Component hex in SampledCells())
        {
            Vector3 center = hex.transform.position;

            foreach (float radius in new[] { NearCenterFactor * step, InsideCellFactor * step })
            {
                for (int i = 0; i < DirectionCount; i++)
                {
                    float angle = i * (2f * Mathf.PI / DirectionCount);
                    var probe = center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;

                    Assert.That(HexAt(probe), Is.SameAs(hex),
                        $"Точка внутри клетки {hex.name} (радиус {radius:F3}, угол {i}) " +
                        "определилась как соседняя клетка.");
                }
            }
        }

        yield break;
    }

    [UnityTest]
    public IEnumerator GetHexAt_FarOutsideBoard_ReturnsNothing()
    {
        Bounds board = BoardBounds();
        var faraway = new Vector3(board.max.x + board.size.x, board.center.y, board.max.z + board.size.z);

        Assert.That(HexAt(faraway), Is.Null);
        yield break;
    }

    private object HexAt(Vector3 worldPosition) => Call(Grid, "GetHexAt", worldPosition);

    private Component[] SampledCells()
    {
        return Hexes.Where((_, index) => index % CellSamplingStep == 0).ToArray();
    }

    private float LatticeStep()
    {
        Vector3 origin = Hexes[0].transform.position;
        float smallest = float.MaxValue;

        foreach (Component hex in Hexes.Skip(1))
        {
            Vector3 offset = hex.transform.position - origin;
            float distance = new Vector2(offset.x, offset.z).magnitude;

            if (distance < smallest)
                smallest = distance;
        }

        return smallest;
    }

    private Bounds BoardBounds()
    {
        var bounds = new Bounds(Hexes[0].transform.position, Vector3.zero);

        foreach (Component hex in Hexes)
            bounds.Encapsulate(hex.transform.position);

        return bounds;
    }
}
