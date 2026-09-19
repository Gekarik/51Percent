using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class HexView : MonoBehaviour, IHexView, ICoroutineRunner
{
    [Required] [SerializeField] private HexViewSettings _viewSettings;
    [Required] [SerializeField] private MeshRenderer _outlineRenderer;

    private Mesh _normalMesh;
    private HexViewAnimator _hexViewAnimator;
    private Colorizer _colorizer;
    private MeshRenderer _meshRenderer;
    private MeshFilter _meshFilter;

    // Ленивый доступ: границы меша могут запрашиваться из HexGrid.Awake раньше, чем отработает наш Awake
    private MeshRenderer MeshRenderer => _meshRenderer != null ? _meshRenderer : _meshRenderer = GetComponent<MeshRenderer>();
    private MeshFilter MeshFilter => _meshFilter != null ? _meshFilter : _meshFilter = GetComponent<MeshFilter>();

    private void Awake()
    {
        _normalMesh = MeshFilter.sharedMesh;
        _hexViewAnimator = new HexViewAnimator(transform, _viewSettings);
        _colorizer = new Colorizer(MeshRenderer, this, _viewSettings);
        _outlineRenderer.enabled = false;
    }

    public Bounds GetBounds() => MeshRenderer.bounds;
    public Bounds GetLocalMeshBounds() => MeshFilter.sharedMesh.bounds;

    public void SetMesh(Mesh mesh) => MeshFilter.sharedMesh = mesh != null ? mesh : _normalMesh;

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
