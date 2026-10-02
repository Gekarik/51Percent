using System;
using UnityEngine;

[RequireComponent(typeof(Mover), typeof(Conqueror))]
[RequireComponent(typeof(Grabber), typeof(VectorProviderComponent))]
[RequireComponent(typeof(SpawnDescentAnimator))]
[RequireComponent(typeof(BoosterPresentation))]

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
    private VectorProviderComponent _vectorProvider;
    private Grabber _grabber;
    private ColorService _colorService;
    private RagdollController _ragdollController;
    private TrailAppearance _trailAppearance;
    private IMatchState _matchState;
    private IHexGridProvider _grid;
    private LandingWindow _landing;
    private SpawnDescentAnimator _descentAnimator;
    private BoosterPresentation _boosterPresentation;

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

    public PlayerStats LifeStats { get; } = new PlayerStats();

    public BoosterHandler Boosters { get; private set; }

    private void Awake()
    {
        _conqueror = GetComponent<Conqueror>();
        _mover = GetComponent<Mover>();
        _vectorProvider = GetComponent<VectorProviderComponent>();
        _grabber = GetComponent<Grabber>();
        _ragdollController = GetComponentInChildren<RagdollController>();
        _descentAnimator = GetComponent<SpawnDescentAnimator>();
        _boosterPresentation = GetComponent<BoosterPresentation>();
    }

    private void OnLandingChanged()
    {
        _view.SetLanding(IsLanding);
        ApplyInteractionState();
    }

    public IBoosterObservable BoosterObservable => Boosters;
    public ITrailVisualProvider TrailVisual => _trailAppearance;
    public ITrailObservable Trail => _conqueror;

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

    public void SetTrailMesh(Mesh mesh) => _trailAppearance.SetMesh(mesh);

    public void ClearTrailMesh() => _trailAppearance.ClearMesh();

    public void Init(ColorService colorService, TerritoryManager territoryManager, IHexGridProvider grid,
        IMatchState matchState)
    {
        _matchState = matchState ?? throw new ArgumentNullException(nameof(matchState));
        _matchState.Changed += ApplyInteractionState;
        _colorService = colorService ?? throw new ArgumentNullException(nameof(colorService));
        _grid = grid ?? throw new ArgumentNullException(nameof(grid));

        _trailAppearance = new TrailAppearance();
        _landing = new LandingWindow(_config.LandingDuration);
        _landing.Started += OnLandingChanged;
        _landing.Finished += OnLandingChanged;

        Stats = new CharacterStats();
        Stats.SetBase(StatType.Speed, _config.BaseSpeed);
        Stats.SetBase(StatType.CaptureWidth, _config.BaseCaptureWidth);
        _mover.Init(Stats, _config.RotationSpeed);
        Boosters = new BoosterHandler(this);

        _color = _colorService.GetRandomColor();
        _state = CharacterState.Alive;

        _conqueror.Init(territoryManager, grid, Stats);
        _grabber.ItemDetected += OnItemDetected;
        _view.Init(this);
        _descentAnimator.Init(_view.transform, _landing);
        _boosterPresentation.Init(Boosters, _view, this);
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

    public bool CanAcceptBooster => CanAct && Boosters.CanAccept;

    protected virtual void Update()
    {
        _landing?.Tick(Time.deltaTime);
        Boosters?.Tick(Time.deltaTime);
        SendMoveCommand();
    }

    private void SendMoveCommand()
    {
        _mover.SetMoveDirection(CanAct ? _vectorProvider.GetMoveDirection() : Vector3.zero);
    }

    public void SetVectorProvider(VectorProviderComponent provider)
    {
        _vectorProvider = provider != null ? provider : throw new ArgumentNullException(nameof(provider));
    }

    public void AcceptCoin() => LifeStats.AddCoin();

    public virtual bool TryAcceptBooster(IBoosterEffect effect)
    {
        return Boosters.TryActivate(effect);
    }

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
        Boosters.Clear();
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
