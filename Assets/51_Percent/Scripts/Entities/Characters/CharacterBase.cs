using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Mover), typeof(Conqueror))]
[RequireComponent(typeof(Grabber), typeof(VectorProviderComponent), typeof(BoosterHandler))]
[RequireComponent(typeof(TrailVisualModifier))]
[RequireComponent(typeof(SpawnLandingTracker), typeof(SpawnDescentAnimator))]
[RequireComponent(typeof(BoosterAnimationSwitcher), typeof(BoosterPropAttacher), typeof(BoosterScaleAnimator))]

public abstract class CharacterBase : MonoBehaviour, ICharacter, IBoosterContext, ICollectibleConsumer
{
    private const float EscapeYOffset = 0.1f;

    [Required] [SerializeField] private CharacterView _view;
    [Required] [SerializeField] private Transform _headSocket;
    [Required] [SerializeField] private Transform _backSocket;
    [Required] [SerializeField] private Transform _feetSocket;
    [Required] [SerializeField] private CharacterConfigSO _config;

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
    private IMatchState _matchState;
    private IHexGridProvider _grid;
    private SpawnLandingTracker _landing;
    private SpawnDescentAnimator _descentAnimator;
    private BoosterAnimationSwitcher _animationSwitcher;
    private BoosterPropAttacher _propAttacher;
    private BoosterScaleAnimator _scaleAnimator;

    public bool IsLanding => _landing.IsLanding;
    public bool CanAct => _matchState != null && _matchState.IsRunning
        && _state == CharacterState.Alive && !IsLanding;

    public event Action LandingFinished
    {
        add => _landing.Finished += value;
        remove => _landing.Finished -= value;
    }

    public abstract bool IsHuman { get; }
    public CharacterStats Stats { get; private set; }

    // Статистика живёт столько же, сколько экземпляр персонажа; респавн создаёт новую модель.
    public PlayerStats LifeStats { get; } = new PlayerStats();

    private void Awake()
    {
        _conqueror = GetComponent<Conqueror>();
        _mover = GetComponent<Mover>();
        _grabber = GetComponent<Grabber>();
        _ragdollController = GetComponentInChildren<RagdollController>();
        _boosterHandler = GetComponent<BoosterHandler>();
        _trailVisualModifier = GetComponent<TrailVisualModifier>();
        _landing = GetComponent<SpawnLandingTracker>();
        _descentAnimator = GetComponent<SpawnDescentAnimator>();
        _animationSwitcher = GetComponent<BoosterAnimationSwitcher>();
        _propAttacher = GetComponent<BoosterPropAttacher>();
        _scaleAnimator = GetComponent<BoosterScaleAnimator>();
        _landing.Started += OnLandingChanged;
        _landing.Finished += OnLandingChanged;
    }

    // Единственное место, где открытое окно приземления превращается
    // в состояние анимации и в запрет на взаимодействия
    private void OnLandingChanged()
    {
        _view.SetLanding(IsLanding);
        ApplyInteractionState();
    }

    public IBoosterObservable BoosterObservable => _boosterHandler;
    public ITrailVisualProvider TrailVisual => _trailVisualModifier;

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

    public event Action<ICharacter, IReadOnlyList<IHex>> AreaCaptured
    {
        add => _conqueror.AreaCaptured += value;
        remove => _conqueror.AreaCaptured -= value;
    }

    public float Speed => _mover.Velocity.magnitude;

    public Color Color => _color;
    public Transform Transform => transform;

    public Transform GetSocket(SocketType socket) => socket switch
    {
        SocketType.Head => _headSocket,
        SocketType.Back => _backSocket,
        SocketType.Feet => _feetSocket,
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

    public void RegisterDeathInterceptor(Func<bool> interceptor)
    {
        _killManager.RegisterDeathInterceptor(this, interceptor);
    }

    public void UnregisterDeathInterceptor()
    {
        _killManager.UnregisterDeathInterceptor(this);
    }

    // Спасение от смерти: трейл возвращается прежним владельцам, персонаж уносится на свою территорию
    public void EscapeToTerritory()
    {
        _conqueror.AbandonTrail();
        _mover.TeleportTo(PickRefugeHex().Transform.position + Vector3.up * EscapeYOffset);
    }

    private IHex PickRefugeHex()
    {
        var territory = _conqueror.FixedHexes;
        if (territory.Count == 0)
            return _grid.GetRandomHex();

        int index = UnityEngine.Random.Range(0, territory.Count);
        foreach (var hex in territory)
            if (index-- == 0)
                return hex;

        return _grid.GetRandomHex();
    }

    public void SetTrailMesh(Mesh mesh) => _trailVisualModifier.SetMesh(mesh);

    public void ClearTrailMesh() => _trailVisualModifier.ClearMesh();

    public void Init(ColorService colorService, TerritoryManager territoryManager, IHexGridProvider grid,
        KillManager killManager, IMatchState matchState)
    {
        _matchState = matchState ?? throw new ArgumentNullException(nameof(matchState));
        _matchState.Changed += ApplyInteractionState;
        _killManager = killManager ?? throw new ArgumentNullException(nameof(killManager));
        _colorService = colorService ?? throw new ArgumentNullException(nameof(colorService));
        _grid = grid ?? throw new ArgumentNullException(nameof(grid));

        Stats = new CharacterStats();
        Stats.SetBase(StatType.Speed, _config.BaseSpeed);
        Stats.SetBase(StatType.CaptureWidth, _config.BaseCaptureWidth);
        _mover.Init(Stats, _config);
        _boosterHandler.Init(this);

        _color = _colorService.GetRandomColor();
        _state = CharacterState.Alive;

        _conqueror.Init(territoryManager, grid, Stats);
        _grabber.ItemDetected += OnItemDetected;
        _view.Init(this);
        _descentAnimator.Init(_view.transform, _landing);
        _animationSwitcher.Init(_boosterHandler, _view);
        _propAttacher.Init(_boosterHandler, this);
        _scaleAnimator.Init(_boosterHandler, _view);
        _landing.Begin();

        OnInit();
    }

    protected virtual void OnInit() { }

    private void OnDisable()
    {
        if (_grabber != null)
            _grabber.ItemDetected -= OnItemDetected;
    }

    private void OnDestroy()
    {
        if (_matchState != null)
            _matchState.Changed -= ApplyInteractionState;

        if (_landing != null)
        {
            _landing.Started -= OnLandingChanged;
            _landing.Finished -= OnLandingChanged;
        }
    }

    public bool CanAcceptBooster => CanAct && _boosterHandler.CanAccept;
    protected bool TryStorePendingBooster(IBoosterEffect effect) => _boosterHandler.TryStore(effect);
    public bool TryActivatePendingBooster() => _boosterHandler.TryActivatePending();

    public void AcceptCoin() => LifeStats.AddCoin();

    public virtual bool TryAcceptBooster(IBoosterEffect effect)
    {
        return _boosterHandler.TryActivate(effect);
    }

    // Предмет потребляется, только если персонаж его принял — иначе остаётся на поле
    private void OnItemDetected(ICollectible item)
    {
        if (CanAct && item.TryApplyTo(this))
            item.Collect();
    }

    private void ApplyInteractionState()
    {
        bool canInteract = CanAct;
        _mover.enabled = canInteract;
        _grabber.enabled = canInteract;
        _conqueror.enabled = canInteract;
    }

    public bool TryDie()
    {
        if (_state == CharacterState.Died)
            return false;

        _state = CharacterState.Died;
        _landing.Cancel();
        _boosterHandler.Clear();
        _mover.enabled = false;
        _grabber.enabled = false;
        _conqueror.enabled = false;
        _conqueror.ReleaseTerritory();

        _colorService?.ReturnColor(_color);
        _ragdollController.Activate(_color);
        return true;
    }

    public void Kill()
    {
        LifeStats.AddKill();
    }
}
