using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Mover), typeof(Conquester))]
public class Player : MonoBehaviour, ICharacter
{
    private Conquester _conquester;
    private Mover _mover;
    private Animator _animator;

    public CharacterState State { get; private set; }
    public Color Color { get; private set; } = Color.green;

    public void Init(Hex startHex, HexGrid hexGrid)
    {
        _conquester.Init(hexGrid);
        _conquester.GetStartTerritory(startHex);
    }

    private void Awake()
    {
        _mover = GetComponent<Mover>();
        _animator = GetComponent<Animator>();
        _conquester = GetComponent<Conquester>();

        //Color = Colors.GetFreeColor();
        //id = GetId();
    }

    private void Update()
    {
        _animator.SetFloat("Speed", _mover.PlayerSpeed.magnitude);
    }

    public void Die()
    {
        throw new System.NotImplementedException();
    }

    public void Kill()
    {
        throw new System.NotImplementedException();
    }
}

public enum CharacterState
{
    Alive = 0,
    Died
}