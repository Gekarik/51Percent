using UnityEngine;

[RequireComponent(typeof(Mover), typeof(Conquester))]
[RequireComponent(typeof(PlayerStatsComponent))]
public class Player : MonoBehaviour, ICharacter
{
    [field: SerializeField] public Camera Camera { get; private set; }
    [SerializeField] private Animator _animator;
    [SerializeReference] private IVectorProvider _directionProvidier;

    private Conquester _conquester;
    private Mover _mover;
    private Grabber _grabber;
    public PlayerStatsComponent StatsComponent { get; private set; }

    public CharacterState State { get; private set; }
    public Color Color { get; private set; } = Color.green;

    private void OnEnable()
    {
        _grabber.CoinCollected += OnCoinCollected;
        _grabber.BoosterCollected += TempBoosterMethod;
    }

    private void OnDisable()
    {
        _grabber.CoinCollected -= OnCoinCollected;
        _grabber.BoosterCollected -= TempBoosterMethod;
    }

    private void TempBoosterMethod(Booster booster)
    {
        Debug.Log("Some booster collected. Kill me already lol");
    }

    private void OnMakeAKill()
    {
        StatsComponent.RegisterKill();
    }

    private void OnCoinCollected(Coin coin)
    {
        StatsComponent.CollectCoin();
    }

    public void Init(Hex startHex, HexGrid hexGrid)
    {
        _conquester.Init(hexGrid);
        _conquester.GetStartTerritory(startHex);
    }

    private void Awake()
    {
        Camera = Camera.main;
        Camera.GetComponent<CameraFollower>().Init(transform);
        _grabber = GetComponent<Grabber>();
        _mover = GetComponent<Mover>();
        _conquester = GetComponent<Conquester>();
        StatsComponent = GetComponent<PlayerStatsComponent>();

        //Color = Colors.GetFreeColor();
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