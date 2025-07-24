using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class HexView : MonoBehaviour
{
    private const string _Color = nameof(_Color);

    [SerializeField] private Outline _outlineView;

    private IHex _hex;
    private MeshRenderer _meshRenderer;
    private MaterialPropertyBlock _mpbBase;
    private MaterialPropertyBlock _defaultMaterial;

    private void Awake()
    {
        _mpbBase = new MaterialPropertyBlock();
        _defaultMaterial = new MaterialPropertyBlock();

        _meshRenderer = GetComponent<MeshRenderer>();
        
        _meshRenderer.GetPropertyBlock(_defaultMaterial, 0);
        _outlineView.gameObject.SetActive(false);
    }


    private void OnDisable() => _hex.StateChanged -= UpdateView;

    public void Init(IHex hex)
    {
        _hex = hex;
        _hex.StateChanged += UpdateView;
    }

    public Bounds GetBounds() => GetComponent<MeshRenderer>().bounds;

    private void UpdateView(ICharacter player)
    {
        _mpbBase.Clear();

        if(player != null)
            _mpbBase.SetColor(_Color, player.Color);

        _meshRenderer.SetPropertyBlock(_mpbBase, 0);

        _outlineView.gameObject.SetActive(_hex.State == HexState.PartOfTrail);
    }

    public void Reset()
    {
        _meshRenderer.SetPropertyBlock(_defaultMaterial, 0);
        _outlineView.gameObject.SetActive(false);
    }

}
