using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TerritoryOperationTests : GameplayScenario
{
    [UnityTest]
    public IEnumerator CaptureSplittingEnemyTerritory_PublishesOnlyAfterDetachedFragmentIsReleased()
    {
        Call(Territory, "OnCharacterDied", Enemy);
        var center = Hexes.First(hex =>
        {
            var nearby = ((IEnumerable)Call(Grid, "GetHexesInRadius", Read(hex, "Coord"), 3)).Cast<Component>().ToArray();
            return nearby.Length == 37 && nearby.All(cell => Read(cell, "Owner") == null);
        });
        var ring = Neighbors(center);
        var outer = ring.SelectMany(Neighbors).Distinct()
            .Where(hex => (int)Call(Grid, "CalculateDistance", center, hex) == 2).ToArray();
        var mainRoot = outer.First();
        var main = Neighbors(mainRoot).Where(hex => (int)Call(Grid, "CalculateDistance", center, hex) == 3)
            .Take(2).Append(mainRoot).ToArray();
        var detachedRoot = outer.First(hex => main.All(cell => (int)Call(Grid, "CalculateDistance", cell, hex) >= 4));
        var detachedEnd = Neighbors(detachedRoot).First(hex => (int)Call(Grid, "CalculateDistance", center, hex) == 3
            && main.All(cell => (int)Call(Grid, "CalculateDistance", cell, hex) >= 3));
        var detached = new[] { detachedRoot, detachedEnd };
        var bridges = new[]
        {
            ring.First(hex => Neighbors(hex).Contains(mainRoot)),
            ring.First(hex => Neighbors(hex).Contains(detachedRoot))
        };
        // До трейла обе ветки соединены через центр и две клетки будущего кольца.
        Call(Territory, "FixHexes", Enemy, HexArray(main.Concat(detached).Concat(bridges).Append(center)));
        foreach (var hex in ring)
            Call(Territory, "TrailHex", Player, hex);

        int notifications = 0;
        Observe(Territory, "OwnershipChanged", () =>
        {
            notifications++;
            Assert.That(Read(center, "Owner"), Is.SameAs(Player));
            Assert.That(OwnedCount(Enemy), Is.EqualTo(3));
            Assert.That(main.All(hex => ReferenceEquals(Read(hex, "Owner"), Enemy)), Is.True);
            Assert.That(detached.All(hex => Read(hex, "Owner") == null), Is.True);
        });
        Call(Territory, "CaptureEnclosedArea", Player, HexArray(ring));
        Assert.That(notifications, Is.EqualTo(1));
        yield return null;
    }

    [UnityTest]
    public IEnumerator OwnershipBelowThreshold_DoesNotFinish_UntilFirstCellAtOrAbove51Percent()
    {
        Call(Territory, "OnCharacterDied", Player);
        int required = Mathf.CeilToInt(Hexes.Length * 0.51f);
        int finishes = 0;
        ObserveResult(WinConditions, "GameFinished", winner =>
        {
            finishes++;
            Assert.That(winner, Is.SameAs(Player));
        });

        Call(Territory, "FixHexes", Player, HexArray(Hexes.Take(required - 1)));
        Assert.That((float)Call(Territory, "GetOwnershipPercent", Player), Is.LessThan(0.51f));
        Assert.That(finishes, Is.Zero);
        Call(Territory, "FixHex", Player, Hexes[required - 1]);
        Assert.That((float)Call(Territory, "GetOwnershipPercent", Player), Is.GreaterThanOrEqualTo(0.51f));
        Assert.That(finishes, Is.EqualTo(1));
        yield return null;
    }

    [UnityTest]
    public IEnumerator NestedChanges_IgnoreIntermediateMajority_AndPublishTheFinalResultOnce()
    {
        int notifications = 0;
        int finishes = 0;
        object winner = null;
        Observe(Territory, "OwnershipChanged", () => notifications++);
        ObserveResult(WinConditions, "GameFinished", result =>
        {
            finishes++;
            winner = result;
        });
        var majority = Hexes.Take(Mathf.CeilToInt(Hexes.Length * 0.6f)).ToArray();
        using (Changes())
        {
            using (Changes())
                Call(Territory, "FixHexes", Player, HexArray(majority));
            Assert.That(OwnedCount(Player), Is.GreaterThan(Hexes.Length / 2));
            Assert.That(finishes, Is.Zero);
            Assert.That(notifications, Is.Zero);
            Call(Territory, "OnCharacterDied", Player);
        }
        Assert.That(notifications, Is.EqualTo(1));
        Assert.That(finishes, Is.Zero);

        using (Changes())
        {
            Call(Territory, "FixHexes", Player, HexArray(majority));
            Assert.That(finishes, Is.Zero);
        }
        Assert.That(notifications, Is.EqualTo(2));
        Assert.That(finishes, Is.EqualTo(1));
        Assert.That(winner, Is.SameAs(Player));
        float finalPercent = (float)Call(Territory, "GetOwnershipPercent", Player);
        var window = Find("EndgameWindow").Single();
        Assert.That(Read(Field(window, "_territoryText"), "text"), Is.EqualTo($"{finalPercent:P0}"));
        Call(WinConditions, "ForceFinish", Enemy);
        Assert.That(finishes, Is.EqualTo(1));
        yield return null;
    }

    [UnityTest]
    public IEnumerator ClosedRing_CapturesInteriorAndTrail_InOneOwnershipNotification()
    {
        var center = Hexes.First(hex => Read(hex, "Owner") == null
            && Neighbors(hex).Length == 6
            && Neighbors(hex).All(neighbor => Read(neighbor, "Owner") == null && Neighbors(neighbor).Length == 6));
        var ring = Neighbors(center);
        foreach (var hex in ring)
            Call(Territory, "TrailHex", Player, hex);
        int notifications = 0;
        Observe(Territory, "OwnershipChanged", () =>
        {
            notifications++;
            // Подписчик должен видеть уже закреплённые трейл и внутреннюю клетку.
            foreach (var hex in ring.Append(center))
            {
                Assert.That(Read(hex, "Owner"), Is.SameAs(Player));
                Assert.That(Read(hex, "State").ToString(), Is.EqualTo("Busy"));
            }
        });
        var captured = ((IEnumerable)Call(Territory, "CaptureEnclosedArea", Player, HexArray(ring))).Cast<object>().ToArray();
        Assert.That(captured.Length, Is.EqualTo(7));
        Assert.That(captured, Does.Contain(center));
        foreach (var hex in ring.Append(center))
        {
            Assert.That(Read(hex, "Owner"), Is.SameAs(Player));
            Assert.That(Read(hex, "State").ToString(), Is.EqualTo("Busy"));
        }
        Assert.That(notifications, Is.EqualTo(1));
        yield return null;
    }

    [UnityTest]
    public IEnumerator DisconnectedTerritory_KeepsTheLargestFragment_AndIndexMatchesCells()
    {
        Call(Territory, "OnCharacterDied", Player);
        var center = Hexes.First(hex => Read(hex, "Owner") == null && Neighbors(hex).Length == 6
            && Neighbors(hex).All(neighbor => Read(neighbor, "Owner") == null));
        var main = Neighbors(center).Take(2).Append(center).ToArray();
        var island = Hexes.First(hex => Read(hex, "Owner") == null && Neighbors(hex).Length == 6
            && (int)Call(Grid, "CalculateDistance", center, hex) > 6
            && Neighbors(hex).All(neighbor => Read(neighbor, "Owner") == null));
        var detached = new[] { island, Neighbors(island).First() };
        Call(Territory, "FixHexes", Player, HexArray(main.Concat(detached)));
        int notifications = 0;
        Observe(Territory, "OwnershipChanged", () => notifications++);
        Call(Territory, "ReleaseDisconnectedFragments", Player);
        Assert.That(OwnedCount(Player), Is.EqualTo(3));
        Assert.That(Hexes.Count(hex => ReferenceEquals(Read(hex, "Owner"), Player)), Is.EqualTo(3));
        Assert.That(detached.All(hex => Read(hex, "Owner") == null), Is.True);
        Assert.That(notifications, Is.EqualTo(1));
        yield return null;
    }

    [UnityTest]
    public IEnumerator InterruptedScope_PublishesItsCommittedChanges_AndDoesNotBlockLaterOperations()
    {
        int notifications = 0;
        Observe(Territory, "OwnershipChanged", () => notifications++);
        var empty = Hexes.Where(hex => Read(hex, "Owner") == null).Take(2).ToArray();
        Assert.Throws<InvalidOperationException>(() =>
        {
            using (Changes())
            {
                Call(Territory, "FixHex", Player, empty[0]);
                throw new InvalidOperationException("Проверка закрытия пакета при исключении.");
            }
        });
        Assert.That(notifications, Is.EqualTo(1));
        Call(Territory, "FixHex", Player, empty[1]);
        Assert.That(notifications, Is.EqualTo(2));
        yield return null;
    }
}
