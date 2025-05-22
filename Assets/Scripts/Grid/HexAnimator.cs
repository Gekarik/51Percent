using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public class HexAnimator : MonoBehaviour, IHexAnimationHandler
{
    [Header("Bounce Settings")]
    [SerializeField, Tooltip("Высота прыжка при фиксации гекса")] private float _bounceHeight = 1f;
    [SerializeField, Tooltip("Длительность прыжка")] private float _bounceDuration = 0.5f;
    [SerializeField, Tooltip("Количество подпрыгиваний")]
    private int _bounceJumps = 1;

    [Header("Stretch Settings")]
    [SerializeField, Tooltip("Масштаб растяжения")]
    private Vector3 _stretchScale = new Vector3(1.2f, 0.8f, 1f);
    [SerializeField, Tooltip("Длительность растяжения")] private float _stretchDuration = 0.2f;

    private Vector3 _initialScale;

    private void Awake()
    {
        _initialScale = transform.localScale;
    }
    public Tween Bounce()
    {
        return transform.DOJump(transform.position, _bounceHeight, _bounceJumps, _bounceDuration)
        .SetEase(Ease.OutQuad);
    }

    public Tween Stretch()
    {
        return DOTween.Sequence()
                      .Append(transform.DOScale(_stretchScale, _stretchDuration).SetEase(Ease.OutQuad))
                      .Append(transform.DOScale(_initialScale, _stretchDuration).SetEase(Ease.InQuad));
    }

    public void Stop()
    {
        throw new System.NotImplementedException();
    }

    public void Reset()
    {
        throw new System.NotImplementedException();
    }
}
