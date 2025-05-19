using DG.Tweening;
using UnityEngine;

public class Hex : MonoBehaviour
{
    public int OwnerId { get; private set; }
    public AxialCoord Coord { get; private set; }
    private Renderer _renderer;

    private Animator _animator;
    private MaterialPropertyBlock _block;
    private Transform _transform;

    private void Awake()
    {
        _transform = transform;
        _animator = GetComponent<Animator>();
        _block = new MaterialPropertyBlock();
        _renderer = GetComponent<Renderer>();
        _block = new MaterialPropertyBlock();
    }

    public void SetOwner(int playerId, Color color)
    {
        if ((playerId == OwnerId) || playerId < 0)
            return;
        OwnerId = playerId;
        SetColor(color);
    }

    public void SetColor(Color color)
    {
        _renderer.GetPropertyBlock(_block);
        _block.SetColor("_Color", color);
        _renderer.SetPropertyBlock(_block);
    }

    public void SetCoord(AxialCoord axialCoord)
    {
        Coord = axialCoord;
    }
}