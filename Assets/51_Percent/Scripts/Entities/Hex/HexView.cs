using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class HexView : MonoBehaviour, ICoroutineRunner
{
    [SerializeField] private Outline _outlineView;
    [SerializeField] private HexViewSettings _viewSettings;

    private HexViewAnimator _hexViewAnimator;
    private Colorizer _colorizer;
    private MeshRenderer _meshRenderer;

    private MeshRenderer MeshRenderer
    {
        get
        {
            if (_meshRenderer == null)
                _meshRenderer = GetComponent<MeshRenderer>();
            return _meshRenderer;
        }
    }

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();

        _hexViewAnimator = new HexViewAnimator(transform, _viewSettings);
        _colorizer = new Colorizer(_meshRenderer, this, _viewSettings);
        _outlineView.gameObject.SetActive(false);
    }

    public Bounds GetBounds() => MeshRenderer.bounds;

    public void UpdateView(IHex hex)
    {
        _outlineView.gameObject.SetActive(hex.State == HexState.PartOfTrail);

        if (hex.State == HexState.Empty)
            _colorizer.ResetColor();

        if (hex.State == HexState.Busy)
        {
            _colorizer.SetColorSlowly(hex.Owner.Color);
        }

        if (hex.State == HexState.PartOfTrail)
        {
            _hexViewAnimator.Pulse();
            _colorizer.SetColorInstantly(hex.Owner.Color);
        }
    }

    public void Reset()
    {
        _colorizer.ResetColor();
        _outlineView.gameObject.SetActive(false);
    }

    public Coroutine StartRoutine(IEnumerator routine) => StartCoroutine(routine);
    public void StopRoutine(Coroutine routine) => StopCoroutine(routine);
}
