using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Mover))]
public class Player : MonoBehaviour
{
    public Color Color { get; private set; } = Color.green;
    private Conquester _conquester;
    private Mover _mover;
    private Animator _animator;
    public Hex _startHex { get; private set; }

    public int Id { get; private set; } = 5;

    public void Init(Hex startHex)
    {
        _startHex = startHex;
    }

    private void Awake()
    {
        _mover = GetComponent<Mover>();
        _animator = GetComponent<Animator>();
        _conquester = GetComponent<Conquester>();
    }

    private void Update()
    {
        _animator.SetFloat("Speed", _mover.PlayerSpeed.magnitude);
    }

    public void Die()
    {

    }
}
