using System;
using UnityEngine;

[RequireComponent(typeof(Mover), typeof(Conquester), typeof(PlayerStatsComponent))]
[RequireComponent(typeof(Grabber), typeof(VectorProviderComponent))]
public abstract class CharacterAbstract : MonoBehaviour, ICharacter
{
    [SerializeField] private CharacterView _view;

    private CharacterState _state = CharacterState.Alive;
    private Color _color;
    private Conquester _conquester;
    private Mover _mover;
    private Grabber _grabber;

    public Conquester Conquester => _conquester;

    public float Speed => _mover.PlayerSpeed.magnitude;
    public PlayerStatsComponent StatsComponent { get; private set; }
    public Color Color => _color;
    public Transform Transform => transform;
    public CharacterState State => _state;

    public void InitConquester(IHexGridProvider hexGrid)
    {
        _conquester.Init(hexGrid);
    }

    protected void BaseInit()
    {
        _color = ColorManager.GetRandomColor();
        SetCharacterState(CharacterState.Alive);

        _conquester = GetComponent<Conquester>();
        _mover = GetComponent<Mover>();
        
        _grabber = GetComponent<Grabber>();
        _grabber.ItemCollected += OnItemCollected;
        
        StatsComponent = GetComponent<PlayerStatsComponent>();
        _view.Init(this);
    }
    
    private void OnDisable()
    {
        _grabber.ItemCollected -= OnItemCollected;
    }

    private void OnItemCollected(IGrabbable item)
    {
        switch (item)
        {
            case Coin:
                StatsComponent.CollectCoin();
                break;

            case Booster:
                break;
        }
    }
    
    protected void SetCharacterState(CharacterState state)
    {
        if (_state == state)
            return;

        _state = state;
    }

    public void Die()
    {
        SetCharacterState(CharacterState.Died);
        _mover.enabled = false;
        _grabber.enabled = false;
        _conquester.enabled = false;
        _conquester.Reset();
        gameObject.SetActive(false);
    }

    public void Kill()
    {
        StatsComponent.RegisterKill();
    }

    protected void Reset()
    {
        _mover.enabled = false;
        _grabber.enabled = false;
        _conquester.Reset();
        _conquester.enabled = false;
    }
}
