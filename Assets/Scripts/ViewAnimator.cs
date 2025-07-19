using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ViewAnimator : MonoBehaviour
{
    [SerializeField] private float animationHeight = 0.25f;
    [SerializeField] float delayIncrement = 0.01f;
    [SerializeField] float animationDuration = 0.3f;

    private void OnEnable()
    {

    }

    private void OnDisable()
    {

    }

    public void Stretch(Transform hex)
    {

    }

    public void Wave(IReadOnlyCollection<Transform> hexes)
    {
        foreach (var (hex, index) in hexes.Select((hex, index) => (hex, index)))
        {
            var startPos = hex.transform.position;
            var jumpUp = startPos + Vector3.up * animationHeight;

            hex.transform
                .DOMoveY(jumpUp.y, animationDuration)
                .SetDelay(index * delayIncrement)
                .OnComplete(() => hex.transform.DOMoveY(startPos.y, animationDuration)
                );
        }
    }
}
