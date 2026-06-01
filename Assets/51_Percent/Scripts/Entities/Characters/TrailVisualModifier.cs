using System;
using UnityEngine;

public class TrailVisualModifier : MonoBehaviour, ITrailVisualProvider
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
