using System.Collections;
using UnityEngine;

public interface ICoroutineRunner
{
    Coroutine StartRoutine(IEnumerator routine);
    void StopRoutine(Coroutine routine);
}
