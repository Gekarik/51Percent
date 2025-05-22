using UnityEngine;

public class RayCaster : MonoBehaviour
{
    private RaycastHit _hit;
    private Ray _ray;
    private Camera _camera;
    [SerializeField] private HexGridLegacy _grid;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }
    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            _ray = _camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(_ray, out _hit))
            {
                if (_hit.transform.gameObject.TryGetComponent(out Hex hex))
                {
                    //var test = _grid.GetCoordByHex(hex);
                    //Debug.Log(_grid.GetCoord(hex));
                }
                else
                {
                    Debug.Log(_hit.ToString());
                }
            }
        }
    }
}
