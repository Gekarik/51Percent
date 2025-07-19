using System;
using UnityEngine;

public class Hex : MonoBehaviour
{
    [SerializeField] private HexView _hexView;
    public HexView HexView => _hexView;

    public Action<ICharacter> StateChanged;
    public ICharacter Owner { get; private set; }
    public HexState State { get; private set; }

    private void Awake()
    {
        State = HexState.Empty;
    }

    public void SetOwner(ICharacter player, HexState hexState)
    {
        State = hexState;
        Owner = player ?? throw new ArgumentNullException(nameof(player));

        StateChanged?.Invoke(player);
    }

    public Bounds GetRendererBounds() => _hexView.GetBounds();

    public void Reset()
    {
        Owner = null;
        State = HexState.Empty;
        _hexView.Reset();
    }
}