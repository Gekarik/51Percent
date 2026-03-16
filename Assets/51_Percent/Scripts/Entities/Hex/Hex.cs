using System;
using UnityEngine;

public class Hex : MonoBehaviour, IHex
{
    [SerializeField] private HexView _hexView;

    private HexState _state;

    public event Action<IHex> StateChanged;

    public Transform Transform => transform;
    public HexView HexView => _hexView;
    public Transform ViewTransform => _hexView.transform;
    public HexState State => _state;
    public ICharacter Owner { get; private set; }

    private void Awake()
    {
        _state = HexState.Empty;
        Owner = null;
    }

    private void OnEnable()
    {
        StateChanged += _hexView.UpdateView;
    }

    private void OnDisable()
    {
        StateChanged -= _hexView.UpdateView;
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
        _hexView.Reset();
    }
}
