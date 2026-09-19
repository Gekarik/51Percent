using System;
using UnityEngine;

public class Hex : MonoBehaviour, IHex
{
    [Required] [SerializeField] private HexView _hexView;

    // Координата назначается генератором при создании грида и сериализуется в сцену:
    // гекс — единственный источник правды о своей позиции в сетке
    [SerializeField, HideInInspector] private int _coordQ;
    [SerializeField, HideInInspector] private int _coordR;

    private HexState _state;

    public event Action<IHex> StateChanged;

    public Transform Transform => transform;
    public HexView HexView => _hexView;
    public Transform ViewTransform => _hexView.transform;
    public HexCoord Coord => new HexCoord(_coordQ, _coordR);
    public HexState State => _state;
    public ICharacter Owner { get; private set; }

    private HexPresenter _presenter;

    private void Awake()
    {
        _state = HexState.Empty;
        Owner = null;
        _presenter = new HexPresenter(_hexView);
    }

    private void OnEnable()
    {
        StateChanged += _presenter.OnStateChanged;
    }

    private void OnDisable()
    {
        StateChanged -= _presenter.OnStateChanged;
    }

    public void AssignCoord(HexCoord coord)
    {
        _coordQ = coord.Q;
        _coordR = coord.R;
    }

    public void SetOwner(ICharacter player, HexState hexState)
    {
        Owner = player;
        _state = hexState;
        StateChanged?.Invoke(this);
    }

    public Bounds GetRendererBounds()
    {
        Vector3 meshSize = _hexView.GetLocalMeshBounds().size;
        Vector3 scaledSize = Vector3.Scale(meshSize, transform.lossyScale);
        return new Bounds(transform.position, scaledSize);
    }

    public void Reset()
    {
        SetOwner(null, HexState.Empty);
        _presenter.Reset();
    }
}
