using System;
using UnityEngine;

[RequireComponent(typeof(Mover), typeof(Conqueror), typeof(PlayerStatsComponent))]
[RequireComponent(typeof(Grabber), typeof(VectorProviderComponent), typeof(BoosterHandler))]
[RequireComponent(typeof(TrailVisualModifier))]

public abstract class CharacterBase : MonoBehaviour, ICharacter, IBoosterContext
{
    [SerializeField] private CharacterView _view;
    [SerializeField] private Transform _headSocket;
    [SerializeField] private CharacterConfigSO _config;

    private string _name;
    private CharacterState _state = CharacterState.Alive;
    private Color _color;
    private Conqueror _conqueror;
    private Mover _mover;
    private Grabber _grabber;
    private ColorService _colorService;
    private RagdollController _ragdollController;
    private BoosterHandler _boosterHandler;
    private TrailVisualModifier _trailVisualModifier;
    private KillManager _killManager;

    public abstract bool IsHuman { get; }
    public CharacterStats Stats { get; private set; }

    private void Awake()
    {
        _conqueror = GetComponent<Conqueror>();
        _mover = GetComponent<Mover>();
        _grabber = GetComponent<Grabber>();
        _ragdollController = GetComponentInChildren<RagdollController>();
        _boosterHandler = GetComponent<BoosterHandler>();
        _trailVisualModifier = GetComponent<TrailVisualModifier>();
        StatsComponent = GetComponent<PlayerStatsComponent>();
    }
    public IBoosterObservable BoosterObservable => _boosterHandler;
    public ITrailVisualProvider TrailVisual => _trailVisualModifier;

    public event Action Died;
    public event Action RespawnRequested;

    public bool HasActiveTrail => _conqueror.TrailHexes.Count > 0;

    public event Action<ICharacter, ICharacter> TrailInterrupted
    {
        add => _conqueror.TrailInterrupted += value;
        remove => _conqueror.TrailInterrupted -= value;
    }

    public event Action<ICharacter> TrailOrphaned
    {
        add => _conqueror.TrailOrphaned += value;
        remove => _conqueror.TrailOrphaned -= value;
    }

    public float Speed => _mover.PlayerSpeed.magnitude;

    public PlayerStatsComponent StatsComponent { get; private set; }
    public Color Color => _color;
    public Transform Transform => transform;

    public Transform GetSocket(SocketType socket) => socket switch
    {
        SocketType.Head => _headSocket,
        _ => throw new ArgumentOutOfRangeException(nameof(socket))
    };

    public CharacterState State => _state;
    public string Name => _name;

    public void SetName(string name)
    {
        _name = name;
    }

    public void RegisterTrailKillResolver(Func<ICharacter, ICharacter, (ICharacter victim, ICharacter killer)> resolver)
    {
        _killManager.RegisterResolver(this, resolver);
    }

    public void UnregisterTrailKillResolver()
    {
        _killManager.UnregisterResolver(this);
    }

    public void SetModelScale(float factor) => _view.SetModelScale(factor);

    public void SetTrailMesh(Mesh mesh) => _trailVisualModifier.SetMesh(mesh);

    public void ClearTrailMesh() => _trailVisualModifier.ClearMesh();

    public void Init(ColorService colorService, TerritoryManager territoryManager, IHexGridProvider grid, KillManager killManager)
    {
        _killManager = killManager ?? throw new ArgumentNullException(nameof(killManager));
        _colorService = colorService ?? throw new ArgumentNullException(nameof(colorService));

        Stats = new CharacterStats();
        Stats.SetBase(StatType.Speed, _config.BaseSpeed);
        Stats.SetBase(StatType.CaptureWidth, _config.BaseCaptureWidth);
        _mover.Init(Stats, _config);
        _boosterHandler.Init(this);

        _color = _colorService.GetRandomColor();
        SetCharacterState(CharacterState.Alive);

        _conqueror.Init(territoryManager, grid, Stats);
        _grabber.ItemCollected += OnItemCollected;
        _view.Init(this);

        OnInit();
    }

    public void RequestRespawn()
    {
        RespawnRequested?.Invoke();
    }

    protected virtual void OnInit() { }

    private void OnDisable()
    {
        if (_grabber != null)
            _grabber.ItemCollected -= OnItemCollected;
    }

    protected void StorePendingBooster(IBoosterEffect effect) => _boosterHandler.Store(effect);
    protected void ActivatePendingBooster() => _boosterHandler.ActivatePending();

    protected virtual void OnBoosterCollected(Booster booster) =>
        _boosterHandler.Activate(booster.CreateEffect());

    private void OnItemCollected(ICollectible item)
    {
        switch (item)
        {
            case Coin:
                StatsComponent.CollectCoin();
                break;

            case Booster booster:
                OnBoosterCollected(booster);
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
        _ragdollController.Activate(_color);

        Died?.Invoke();
    }

    public void Kill()
    {
        StatsComponent.RegisterKill();
    }

    protected void ResetState()
    {
        _mover.enabled = false;
        _grabber.enabled = false;
        _conqueror.Reset();
        _conqueror.enabled = false;
    }
}
