using System;
using UnityEngine;

[RequireComponent(typeof(Mover), typeof(Conqueror), typeof(PlayerStatsComponent))]
[RequireComponent(typeof(Grabber), typeof(VectorProviderComponent))]
public abstract class CharacterBase : MonoBehaviour, ICharacter
{
    [SerializeField] private CharacterView _view;

    private string _name;
    private CharacterState _state = CharacterState.Alive;
    private Color _color;
    private Conqueror _conqueror;
    private Mover _mover;
    private Grabber _grabber;
    private ColorService _colorService;

    public Conqueror Conqueror => _conqueror;

    public float Speed => _mover.PlayerSpeed.magnitude;
    
    public PlayerStatsComponent StatsComponent { get; private set; }
    public Color Color => _color;
    public Transform Transform => transform;
    public CharacterState State => _state;

    public string Name => _name;

    public void SetName(string name)
    {
        _name = name;
    }

    public void Init(ColorService colorService, TerritoryManager territoryManager, IHexGridProvider grid)
    {
        _colorService = colorService ?? throw new ArgumentNullException(nameof(colorService));

        _conqueror = GetComponent<Conqueror>();
        _mover = GetComponent<Mover>();
        _grabber = GetComponent<Grabber>();
        StatsComponent = GetComponent<PlayerStatsComponent>();

        _color = _colorService.GetRandomColor();
        SetCharacterState(CharacterState.Alive);

        _conqueror.Init(territoryManager, grid);
        _grabber.ItemCollected += OnItemCollected;
        _view.Init(this);

        OnInit();
    }

    protected virtual void OnInit() { }

    private void OnDisable()
    {
        if (_grabber != null)
            _grabber.ItemCollected -= OnItemCollected;
    }

    private void OnItemCollected(ICollectible item)
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
        _conqueror.enabled = false;
        _conqueror.Reset();

        _colorService?.ReturnColor(_color);

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
        _conqueror.Reset();
        _conqueror.enabled = false;
    }
}
