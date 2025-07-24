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

    //public event Action Died;

    public float Speed => _mover.PlayerSpeed.magnitude;
    public PlayerStatsComponent StatsComponent { get; private set; }
    public Color Color => _color;
    public CharacterState State => _state;

    public void InitConquester(IHexGridProvider hexGrid, Hex startHex)
    {
        _conquester.Init(hexGrid);
        _conquester.GetStartTerritory(startHex);
    }

    protected void BaseInit()
    {
        _color = ColorManager.GetRandomColor();

        SetCharacterState(CharacterState.Alive);

        _mover = GetComponent<Mover>();
        _conquester = GetComponent<Conquester>();

        _grabber = GetComponent<Grabber>();
        InitGrabberEvents();

        StatsComponent = GetComponent<PlayerStatsComponent>();

        _view.Init(this);
    }

    private void InitGrabberEvents()
    {
        _grabber.CoinCollected += OnCoinCollected;
        _grabber.BoosterCollected += OnBoosterCollected;
    }

    private void OnDisable()
    {
        _grabber.CoinCollected -= OnCoinCollected;
        _grabber.BoosterCollected -= OnBoosterCollected;
    }

    private void OnCoinCollected(Coin coin)
    {
        StatsComponent.CollectCoin();
    }

    private void OnBoosterCollected(Booster booster)
    {
        throw new System.NotImplementedException();
    }

    protected void SetCharacterState(CharacterState state)
    {
        if (_state == state)
            return;

        _state = state;
    }

    public void Die()
    {
        _conquester.Reset();
        SetCharacterState(CharacterState.Died);
        gameObject.SetActive(false);
    }

    public void Kill()
    {
        StatsComponent.RegisterKill();
    }

    protected void Reset()
    {
        _conquester.Reset();
        _mover.enabled = false;
        _grabber.enabled = false;
    }
}
