using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    [SerializeField] public Vector3 offset;
    [SerializeField] public float smoothSpeed = 0.125f;

    private Transform _player;
    public void Init(Transform transform) => _player = transform;

    private void LateUpdate()
    {
        if (_player != null)
        {
            Vector3 desiredPosition = _player.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
        }
    }
}
