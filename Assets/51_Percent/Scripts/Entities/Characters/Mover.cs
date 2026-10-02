using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class Mover : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private CharacterStats _stats;
    private float _rotationSpeed;
    private Vector3 _direction;

    public Vector3 Velocity => _rigidbody.velocity;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();

        _rigidbody.constraints |= RigidbodyConstraints.FreezePositionY;
    }

    public void Init(CharacterStats stats, float rotationSpeed)
    {
        _stats = stats;
        _rotationSpeed = rotationSpeed;
    }

    public void SetMoveDirection(Vector3 direction)
    {
        _direction = direction;
    }

    private void FixedUpdate()
    {
        Move();
        Rotate();
    }

    private void OnDisable()
    {
        _direction = Vector3.zero;
        if (_rigidbody != null)
            _rigidbody.velocity = Vector3.zero;
    }

    private void Move()
    {
        float speed = _stats.GetValue(StatType.Speed);
        Vector3 desired = _direction * speed;
        _rigidbody.velocity = new Vector3(desired.x, 0f, desired.z);
    }

    public void TeleportTo(Vector3 position)
    {
        _rigidbody.position = position;
    }

    private void Rotate()
    {
        if (_direction.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(_direction);
        float step = _rotationSpeed * Time.fixedDeltaTime;
        _rigidbody.MoveRotation(Quaternion.RotateTowards(_rigidbody.rotation, targetRot, step));
    }
}
