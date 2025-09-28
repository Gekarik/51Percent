using System.Collections.Generic;
using UnityEngine;

public interface IWaveAnimator
{
    void Wave(IReadOnlyCollection<Transform> transforms);
    void Reset();
}
