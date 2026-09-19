using System.Collections.Generic;
using UnityEngine;

public interface IWaveAnimator
{
    void Wave(IReadOnlyCollection<Transform> transforms, Vector3 origin);
}
