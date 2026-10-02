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

        float lerpFactor = 1f - Mathf.Exp(-_damping * Time.unscaledDeltaTime);
        Vector3 desiredPosition = _target.position + _offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, lerpFactor);
    }
}
