using System;
using UnityEngine;

public class Hex : MonoBehaviour
{
    [SerializeField] private HexView _hexView;
    [SerializeField] private AxialCoord _coord;
    public AxialCoord Coord => _coord;

    public Action<ICharacter> StateChanged;
    public ICharacter Owner { get; private set; }
    public HexState State { get; private set; }

    private void Awake()
    {
        State = HexState.Empty;
    }

    public void SetOwner(ICharacter player, HexState hexState)
    {
        Debug.Log("Owner Setted");
        State = hexState;
        Owner = player ?? throw new ArgumentNullException(nameof(player));
        StateChanged?.Invoke(player);
    }

    public void SetCoord(AxialCoord coord) => _coord = coord;

    public Bounds GetRendererBounds() => _hexView.GetBounds();

    public void Reset()
    {
        Owner = null;
        State = HexState.Empty;
        _hexView.Reset();
    }
}
