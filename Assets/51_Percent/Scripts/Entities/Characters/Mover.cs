using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(VectorProviderComponent))]

public class Mover : MonoBehaviour
{
    private VectorProviderComponent _vectorProvider;
    private Rigidbody _rigidbody;
    private CharacterStats _stats;
    private float _rotationSpeed;
    private Vector3 _direction;
    public Vector3 PlayerSpeed { get; private set; }

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _vectorProvider = GetComponent<VectorProviderComponent>();

        if (_vectorProvider == null)
            throw new InvalidOperationException("No VectorProviderComponent");
    }

    public void Init(CharacterStats stats, CharacterConfigSO config)
    {
        _stats = stats;
        _rotationSpeed = config.RotationSpeed;
    }

    private void Update()
    {
        _direction = _vectorProvider.GetMoveDirection();
    }

    private void FixedUpdate()
    {
        Move();
        Rotate();
    }

    private void Move()
    {
        float speed = _stats.GetValue(StatType.Speed);
        Vector3 desired = _direction * speed;
        _rigidbody.velocity = new Vector3(desired.x, _rigidbody.velocity.y, desired.z);
        PlayerSpeed = new Vector3(desired.x, 0f, desired.z);
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
