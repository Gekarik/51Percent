using System;
using UnityEngine;

public class Hex : MonoBehaviour
{
    [SerializeField] private HexView _hexView;

    public Action<ICharacter> StateChanged;
    public Action AddedToTrail;
    public AxialCoord Coord { get; private set; }
    public ICharacter Owner { get; private set; }
    public HexState State { get; private set; }

    private void Awake()
    {
        State = HexState.Empty;
    }

    public void SetOwner(ICharacter player)
    {
        Debug.Log("Owner Setted");
        Owner = player ?? throw new ArgumentNullException(nameof(player));
        State = HexState.Busy;
        StateChanged?.Invoke(player);
    }

    public void SetState(HexState hexState)
    {
        State = hexState;
        AddedToTrail?.Invoke();
    }

    public void SetCoord(AxialCoord coord) => Coord = coord;

    public Bounds GetRendererBounds() => _hexView.GetBounds();

    public void Reset()
    {
        Owner = null;
        State = HexState.Busy;
    }
}
