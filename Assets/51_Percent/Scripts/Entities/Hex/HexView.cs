using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class HexView : MonoBehaviour, IHexView, ICoroutineRunner
{
    [SerializeField] private HexViewSettings _viewSettings;
    [SerializeField] private MeshRenderer _outlineRenderer;

    private Mesh _normalMesh;
    private HexViewAnimator _hexViewAnimator;
    private Colorizer _colorizer;
    private MeshRenderer _meshRenderer;
    private MeshFilter _meshFilter;

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _meshFilter = GetComponent<MeshFilter>();
        _normalMesh = _meshFilter.sharedMesh;
        _hexViewAnimator = new HexViewAnimator(transform, _viewSettings);
        _colorizer = new Colorizer(_meshRenderer, this, _viewSettings);
        _outlineRenderer.enabled = false;
    }

    public Bounds GetBounds() => _meshRenderer.bounds;
    public Bounds GetLocalMeshBounds() => _meshFilter.sharedMesh.bounds;

    public void SetMesh(Mesh mesh) => _meshFilter.sharedMesh = mesh != null ? mesh : _normalMesh;

    public void SetOutline(bool visible) => _outlineRenderer.enabled = visible;

    public void SetColorInstantly(Color color) => _colorizer.SetColorInstantly(color);

    public void SetColorSlowly(Color color) => _colorizer.SetColorSlowly(color);

    public void ResetColor() => _colorizer.ResetColor();

    public void Pulse() => _hexViewAnimator.Pulse();

    public void Reset()
    {
        SetMesh(null);
        SetOutline(false);
        _colorizer.ResetColor();
    }

    public Coroutine StartRoutine(IEnumerator routine) => StartCoroutine(routine);
    public void StopRoutine(Coroutine routine) => StopCoroutine(routine);
}
