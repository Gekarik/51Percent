using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class SpawnPlacementTests : GameplayScenario
{
    [UnityTest]
    public IEnumerator SelectSpawnHex_WhenFreeAreaExists_PicksAreaWithoutForeignOwners()
    {
        Component chosen = SelectSpawnHex();

        Assert.That(Owner(chosen), Is.Null, "Появление заняло бы чужую клетку.");
        Assert.That(ForeignNeighbourCount(chosen), Is.Zero,
            "Стартовая территория отрезала бы клетки у соседей.");
        yield break;
    }

    [UnityTest]
    public IEnumerator SelectSpawnHex_AmongFreeAreas_PicksFarthestFromLivingCharacters()
    {
        Component chosen = SelectSpawnHex();
        float farthest = FreeAreas().Max(DistanceToNearestCharacter);

        Assert.That(DistanceToNearestCharacter(chosen), Is.EqualTo(farthest).Within(0.0001f),
            "Выбрана не самая удалённая из свободных областей.");
        yield break;
    }

    [UnityTest]
    public IEnumerator SelectSpawnHex_NeverPicksForeignTrail()
    {
        // Трейлом врага занимается самая удалённая свободная область: без проверки состояния
        // клетки именно она осталась бы лучшим местом появления.
        Component trapped = FreeAreas().OrderByDescending(DistanceToNearestCharacter).First();

        using (Changes())
        {
            Call(Territory, "TrailHex", Enemy, trapped);
        }

        Assert.That(State(trapped), Is.EqualTo("PartOfTrail"));
        Assert.That(SelectSpawnHex(), Is.Not.SameAs(trapped));
        yield break;
    }

    private Component SelectSpawnHex()
    {
        object selector = New("SpawnHexSelector", Grid, MatchCharacters());

        return (Component)Call(selector, "SelectSpawnHex");
    }

    // Тот же список, что получает спавнер: ботов в сцене несколько,
    // и расстояние должно считаться до всех живых участников
    private object MatchCharacters()
    {
        return Field(Find("PlayerSpawner").Single(), "_allCharacters");
    }

    private Component[] LivingCharacters()
    {
        return ((IEnumerable)MatchCharacters()).Cast<Component>().Where(Alive).ToArray();
    }

    private IEnumerable<Component> FreeAreas()
    {
        return Hexes.Where(hex => State(hex) != "PartOfTrail"
            && Owner(hex) == null
            && ForeignNeighbourCount(hex) == 0);
    }

    private object Owner(Component hex) => Read(hex, "Owner");

    private string State(Component hex) => Read(hex, "State").ToString();

    private int ForeignNeighbourCount(Component hex)
    {
        return Neighbors(hex).Count(neighbour => Owner(neighbour) != null);
    }

    private float DistanceToNearestCharacter(Component hex)
    {
        Vector3 position = hex.transform.position;
        float nearest = float.MaxValue;

        foreach (Component character in LivingCharacters())
        {
            Vector3 offset = character.transform.position - position;
            float distance = offset.x * offset.x + offset.z * offset.z;

            if (distance < nearest)
                nearest = distance;
        }

        return nearest;
    }
}
