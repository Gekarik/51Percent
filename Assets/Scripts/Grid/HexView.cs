using System;
using UnityEngine;

public class HexView : MonoBehaviour
{
    private Hex _hex;
    private MeshRenderer _meshRenderer;
    private MaterialPropertyBlock _propertyBlock;

    private void Awake()
    {
        _propertyBlock = new MaterialPropertyBlock();
        _meshRenderer = GetComponent<MeshRenderer>();
        _hex = GetComponentInParent<Hex>();
    }

    private void OnEnable()
    {
        _hex.StateChanged += OnStateChanged;
    }

    private void OnDisable()
    {
        _hex.StateChanged -= OnStateChanged;
    }

    private void OnStateChanged(ICharacter player)
    {
        switch (_hex.State)
        {
            case HexState.PartOfTrail:
                //Замена цвета и текстуры
                //Stretch();
                break;

            case HexState.Busy:
                //Jump();
                break;

            default:
                break;
        }

        if (_meshRenderer != null)
        {
            _propertyBlock.SetColor("_Color", player.Color);
            _meshRenderer.SetPropertyBlock(_propertyBlock);
        }   
    }

    public Bounds GetBounds() => GetComponent<MeshRenderer>().bounds;

    public void Reset()
    {
        //_meshRenderer.material = _startMaterial;
    }
}
