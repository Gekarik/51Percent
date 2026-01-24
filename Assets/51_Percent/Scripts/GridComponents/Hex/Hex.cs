using System;
using UnityEngine;

public class Hex : MonoBehaviour, IHex
{
    [SerializeField] private HexView _hexView;

    private HexState _state;
    
    public event Action<IHex> StateChanged;
    
    public Transform Transform => transform;
    public HexView HexView => _hexView;
    public HexState State => _state;
    public ICharacter Owner { get; private set; }

    private void OnEnable()
    {
        StateChanged += _hexView.UpdateView;
    }

    private void OnDisable()
    {
        StateChanged -= _hexView.UpdateView;
    }

    private void Awake()
    {
        SetOwner(null, HexState.Empty);
    }

    public void SetOwner(ICharacter player, HexState hexState)
    {
        Owner = player;
        _state = hexState;
        StateChanged?.Invoke(this);
    }

    public Bounds GetRendererBounds() => _hexView.GetBounds();

    public void Reset()
    {
        SetOwner(null, HexState.Empty);
        _hexView.Reset();
    }
}
