using System;
using UnityEngine;

public class Hex : MonoBehaviour, IHex
{
    [SerializeField] private HexView _hexView;
    private HexState _state;

    public event Action<ICharacter> StateChanged;

    public HexView HexView => _hexView;
    public HexState State => _state;
    public ICharacter Owner { get; private set; }

    private void Awake()
    {
        SetState(HexState.Empty);
        _hexView.Init(this);
    }

    public void SetOwner(ICharacter player, HexState hexState)
    {
        Owner = player ?? throw new ArgumentNullException(nameof(player));
        SetState(hexState);
    }

    public Bounds GetRendererBounds() => _hexView.GetBounds();

    private void SetState(HexState state)
    {
        if (state == _state)
            return;

        _state = state;
        
        StateChanged?.Invoke(Owner);
    }

    public void Reset()
    {
        Owner = null;
        SetState(HexState.Empty);
        _hexView.Reset();
    }
}
