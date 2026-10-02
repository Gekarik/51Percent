using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class TrailThreatEvaluatorTests
{
    private const float DetectionRadius = 10f;
    private const float ThreatRadius = 2f;

    private TrailThreatEvaluator _evaluator;

    [SetUp]
    public void SetUp()
    {
        _evaluator = new TrailThreatEvaluator();
    }

    [Test]
    public void EmptyTrail_IsNeverThreatened()
    {
        Assert.That(Threatened(new List<Vector3>(), Positions(Vector3.zero)), Is.False);
    }

    [Test]
    public void RivalNextToFreshTrail_IsThreat()
    {
        var trail = Positions(new Vector3(0f, 0f, 0f), new Vector3(1f, 0f, 0f));

        Assert.That(Threatened(trail, Positions(new Vector3(1f, 0f, 1f))), Is.True);
    }

    [Test]
    public void RivalBeyondDetectionRadius_IsIgnored_EvenWhenStandingOnTrail()
    {
        var trail = Positions(new Vector3(100f, 0f, 0f));

        Assert.That(Threatened(trail, Positions(new Vector3(100f, 0f, 0f))), Is.False);
    }

    [Test]
    public void RivalNearOldTailOnly_IsNotThreat()
    {
        var trail = new List<Vector3>();
        for (int i = 0; i < 12; i++)
            trail.Add(new Vector3(i, 0f, 0f));

        var nearOldTail = Positions(new Vector3(0f, 0f, 0.5f));
        var nearFreshEnd = Positions(new Vector3(11f, 0f, 0.5f));

        Assert.That(Threatened(trail, nearOldTail, botPosition: new Vector3(11f, 0f, 0f)), Is.False,
            "Соперник у давно пройденного начала трейла не должен считаться угрозой.");
        Assert.That(Threatened(trail, nearFreshEnd, botPosition: new Vector3(11f, 0f, 0f)), Is.True);
    }

    [Test]
    public void RivalInsideDetection_ButFarFromTrail_IsNotThreat()
    {
        var trail = Positions(new Vector3(0f, 0f, 0f));

        Assert.That(Threatened(trail, Positions(new Vector3(5f, 0f, 5f))), Is.False);
    }

    private bool Threatened(IReadOnlyList<Vector3> trail, IReadOnlyList<Vector3> rivals, Vector3 botPosition = default)
    {
        return _evaluator.IsTrailThreatened(botPosition, trail, rivals, DetectionRadius, ThreatRadius);
    }

    private static List<Vector3> Positions(params Vector3[] positions) => new List<Vector3>(positions);
}
