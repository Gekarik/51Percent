using DG.Tweening;
using UnityEngine;

public class DoTweenInitializer : MonoBehaviour
{
    [Header("DoTweenSettings")]
    [SerializeField] private bool _recycleAllByDefault;
    [SerializeField] private bool _useSafeMode;
    [SerializeField] private LogBehaviour _logBehaviour;

    [Header("Capacity")]
    [SerializeField] private int _tweenersCapacity;
    [SerializeField] private int _sequencesCapacity;

    private void Awake()
    {
        DOTween.Init(_recycleAllByDefault, _useSafeMode, _logBehaviour).SetCapacity(_tweenersCapacity, _sequencesCapacity);
    }
}
