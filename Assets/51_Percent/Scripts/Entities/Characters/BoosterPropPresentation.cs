using DG.Tweening;
using UnityEngine;

// Срез реестра для прицепа пропов. Отдаём срез, а не целую запись:
// иначе каждый потребитель получает все чужие поля
public readonly struct BoosterPropPresentation
{
    public GameObject Prefab { get; }
    public SocketType Socket { get; }
    public float OutroDuration { get; }
    public Ease OutroEase { get; }

    public BoosterPropPresentation(GameObject prefab, SocketType socket, float outroDuration, Ease outroEase)
    {
        Prefab = prefab;
        Socket = socket;
        OutroDuration = outroDuration;
        OutroEase = outroEase;
    }
}
