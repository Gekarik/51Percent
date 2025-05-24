using System;
using UnityEngine;

[RequireComponent(typeof(ViewAnimator))]
public class HexView : MonoBehaviour
{
    [SerializeField] private Hex _hex;

    private MeshRenderer _meshRenderer;
    private Material _startMaterial;

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _startMaterial = _meshRenderer.material;
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
                //Stretch();
                break;

            case HexState.Busy:
                //Jump();
                break;

            default:
                break;
        }

        if (_meshRenderer != null)
            _meshRenderer.material.color = player.Color;
    }

    public Bounds GetBounds() => GetComponent<MeshRenderer>().bounds;

    public void Reset()
    {
        _meshRenderer.material = _startMaterial;
    }
}
