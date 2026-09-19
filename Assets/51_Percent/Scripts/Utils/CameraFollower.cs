using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class CameraFollower : MonoBehaviour
{
    [FormerlySerializedAs("offset")]
    [SerializeField] private Vector3 _offset;
    [SerializeField] private float _damping = 8f;

    private Transform _target;

    public void Init(Transform target) => _target = target;

    // Плавная смена смещения (наезд endgame-камеры). Когда и куда наезжать — решает вызывающий.
    // Unscaled-твин: наезд доигрывает и при timeScale = 0
    public Tween ZoomTo(Vector3 offset, float duration)
    {
        return DOTween.To(() => _offset, value => _offset = value, offset, duration)
            .SetEase(Ease.InOutSine)
            .SetUpdate(true)
            .SetTarget(this);
    }

    private void LateUpdate()
    {
        if (_target == null)
            return;

        // Экспоненциальное сглаживание, независимое от частоты кадров;
        // unscaled-время — камера обязана двигаться и при остановленном timeScale
        float lerpFactor = 1f - Mathf.Exp(-_damping * Time.unscaledDeltaTime);
        Vector3 desiredPosition = _target.position + _offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, lerpFactor);
    }
}
