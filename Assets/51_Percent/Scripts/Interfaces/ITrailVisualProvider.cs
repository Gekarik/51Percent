using System;
using UnityEngine;

public interface ITrailVisualProvider
{
    Mesh ActiveMesh { get; }
    event Action Changed;
}
