using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CollectiblePickupTests : GameplayScenario
{
    [UnityTest]
    public IEnumerator CollectedCoin_LeavesRegistryImmediately_BeforeItsAnimationEnds()
    {
        Component coin = SpawnCoin();
        object registry = Registry();

        Assert.That(RegisteredCoins(registry), Has.Member(coin),
            "Появившаяся монета должна быть в реестре.");

        Call(coin, "Collect");

        Assert.That(RegisteredCoins(registry), Has.No.Member(coin),
            "Подобранная монета осталась в реестре до конца анимации.");
        Assert.That(coin, Is.Not.Null, "Объект нужен ещё живым: он доигрывает исчезновение.");
        Assert.That(Read(coin, "State").ToString(), Is.EqualTo("Collected"));
        yield break;
    }

    [UnityTest]
    public IEnumerator RepeatedCollect_IsIgnored_AndDoesNotRemoveTwice()
    {
        Component coin = SpawnCoin();
        object registry = Registry();
        int consumedCount = 0;
        ObserveResult(coin, "Consumed", _ => consumedCount++);

        Call(coin, "Collect");
        Call(coin, "Collect");

        Assert.That(consumedCount, Is.EqualTo(1), "Повторный подбор снова выдал награду.");
        Assert.That(RegisteredCoins(registry), Has.No.Member(coin));
        yield break;
    }

    private Component SpawnCoin()
    {
        Component spawner = Find("CoinSpawner").Single();
        var spawned = (object)Call(spawner, "SpawnAt", Player.transform.position + Vector3.right * 3f);

        return (Component)spawned;
    }

    private object Registry() => Field(Find("CoinSpawner").Single(), "_registry");

    private IEnumerable<Component> RegisteredCoins(object registry)
    {
        return ((IEnumerable)Read(registry, "Coins")).Cast<Component>();
    }
}
