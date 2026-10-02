using System;
using UnityEngine;

public interface ICollectible
{
    // Предмет потреблён: награда выдана, с поля он уже выбыл. Визуал в этот момент
    // ещё доигрывает исчезновение, поэтому объект пока существует
    event Action<ICollectible> Consumed;

    // Визуал закончился: объект можно вернуть в пул или уничтожить
    event Action<ICollectible> Collected;
    void Collect();
    bool TryApplyTo(ICollectibleConsumer consumer);
    CollectibleState State { get; }
    Transform Transform { get; }
}
