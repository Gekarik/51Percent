using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Mover : MonoBehaviour
{
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _rotationSpeed = 2f;

    private InputReader _inputReader;
    private Rigidbody _rigidbody;

    private Vector3 _inputVector;
    public Vector3 PlayerSpeed { get; private set; }

    private void Awake()
    {
        _inputReader = new InputReader();
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        _inputReader.UpdateInput();
        _inputVector = new Vector3(_inputReader.MoveX, 0, _inputReader.MoveZ);
    }

    private void FixedUpdate()
    {
        PlayerSpeed = _inputVector * _speed;
        _rigidbody.velocity = PlayerSpeed;

        if (_inputVector.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_inputVector);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.fixedDeltaTime);
        }
    }
}