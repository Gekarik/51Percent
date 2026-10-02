using System;
using UnityEngine;

// Внешний вид трейла персонажа: шипы заменяют меш его клеток на время эффекта.
// Общая точка между бустером, который меш задаёт, и клетками, которые его рисуют —
// поэтому уже выложенный трейл перерисовывается по событию, а не только новый.
// Обычный объект: сцена ему не нужна, владелец — сам персонаж
public class TrailAppearance : ITrailVisualProvider
{
    public Mesh ActiveMesh { get; private set; }

    public event Action Changed;

    public void SetMesh(Mesh mesh)
    {
        ActiveMesh = mesh;
        Changed?.Invoke();
    }

    public void ClearMesh()
    {
        ActiveMesh = null;
        Changed?.Invoke();
    }
}
