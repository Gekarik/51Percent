using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class TerritoryBearingsTests
{
    private TerritoryBearings _bearings;

    [SetUp]
    public void SetUp()
    {
        _bearings = new TerritoryBearings();
    }

    [Test]
    public void NearestPoint_PicksClosestCellOfOwnTerritory()
    {
        var territory = Points(new Vector3(10f, 0f, 0f), new Vector3(2f, 0f, 0f), new Vector3(-7f, 0f, 0f));

        Assert.That(_bearings.NearestPoint(Vector3.zero, territory), Is.EqualTo(new Vector3(2f, 0f, 0f)));
    }

    [Test]
    public void NearestPoint_WithoutTerritory_KeepsBotWhereItIs()
    {
        var bot = new Vector3(3f, 0f, 4f);

        Assert.That(_bearings.NearestPoint(bot, Points()), Is.EqualTo(bot));
        Assert.That(_bearings.NearestPoint(bot, null), Is.EqualTo(bot));
    }

    [Test]
    public void Center_IsAverageOfTerritoryPositions()
    {
        var territory = Points(new Vector3(0f, 0f, 0f), new Vector3(4f, 0f, 0f), new Vector3(2f, 0f, 6f));

        Assert.That(_bearings.Center(Vector3.zero, territory), Is.EqualTo(new Vector3(2f, 0f, 2f)));
    }

    [Test]
    public void Center_WithoutTerritory_FallsBackToGivenPoint()
    {
        var fallback = new Vector3(1f, 0f, 1f);

        Assert.That(_bearings.Center(fallback, Points()), Is.EqualTo(fallback));
    }

    private static List<Vector3> Points(params Vector3[] positions) => new List<Vector3>(positions);
}
