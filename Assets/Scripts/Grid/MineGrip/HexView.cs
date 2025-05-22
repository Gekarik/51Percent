using System;
using UnityEngine;

[RequireComponent(typeof(HexViewAnimator))]
public class HexView : MonoBehaviour
{
    [SerializeField] private Hex _hex;

    private HexViewAnimator _hexViewAnimator;
    private MeshRenderer _meshRenderer;

    private void Awake()
    {
        _hexViewAnimator = GetComponent<HexViewAnimator>();
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    private void OnEnable()
    {
        _hex.StateChanged += OnStateChanged;
        _hex.AddedToTrail += OnAddedToTrail;
    }

    private void OnDisable()
    {
        _hex.StateChanged -= OnStateChanged;
        _hex.AddedToTrail -= OnAddedToTrail;
    }

    private void OnAddedToTrail()
    {
        if (_meshRenderer != null)
            _meshRenderer.material.color = Color.green;
    }

    private void OnStateChanged(ICharacter player)
    {
        if (_meshRenderer != null)
            _meshRenderer.material.color = player.Color;

        switch (_hex.State)
        {
            case HexState.PartOfTrail:
                break;

            case HexState.Busy: 
                break;

            default: 
                break;
        }
    }

    public Bounds GetBounds() => GetComponent<MeshRenderer>().bounds;
}
