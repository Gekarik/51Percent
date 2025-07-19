using System;
using UnityEngine;

[RequireComponent(typeof(Conquester))]
[RequireComponent(typeof(Mover))]
[RequireComponent(typeof(EnemyAIController))]
public class Enemy : MonoBehaviour, ICharacter
{
    [Header("Настройки врага")]
    [SerializeField] private Color _color = Color.red;

    private Conquester _conquester;
    private Mover _mover;
    private EnemyAIController _ai;

    public CharacterState State { get; private set; }
    public Color Color => _color;

    private void Awake()
    {
        _conquester = GetComponent<Conquester>();
        _mover = GetComponent<Mover>();
        _ai = GetComponent<EnemyAIController>();
        State = CharacterState.Alive;
    }

    private void OnEnable()
    {
        _conquester.TrailInterrupted += OnTrailInterrupted;
    }

    private void OnDisable()
    {
        _conquester.TrailInterrupted -= OnTrailInterrupted;
    }

    private void OnTrailInterrupted(ICharacter owner, ICharacter interrupter)
    {
        if (owner == this)
            Die();
    }

    public void Init(IHexGridProvider grid, Hex startHex)
    {
        if (State != CharacterState.Alive)
            throw new InvalidOperationException("Enemy уже инициализирован или мёртв");

        _conquester.Init(grid as HexGrid);
        _conquester.GetStartTerritory(startHex);

        _ai.Init(grid);
    }

    public void Kill()
    {
        Debug.Log($"{name} сделал килл");
    }

    public void Die()
    {
        if (State == CharacterState.Died) return;

        State = CharacterState.Died;
        Debug.Log($"{name} погиб");

        _ai.enabled = false;
        _mover.enabled = false;
        _conquester.enabled = false;

        Destroy(gameObject, 1f);
    }
}
