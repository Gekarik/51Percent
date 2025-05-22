//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;

//[RequireComponent(typeof(Player))]
//public class ConquesterLegacy : MonoBehaviour
//{
//    [Header("Game Objects")]
//    [Tooltip("������ ���� ������ � ����� (����� �������� ������, ����� ����� ���)")]
//    [SerializeField] private List<HexTile> _allHexes;

//    private HexGridLegacy _grid;
//    private Player _player;
//    private readonly List<HexTile> _trail = new();
//    private readonly List<IHexAnimationHandler> _trailAnimators = new();
//    private readonly HashSet<HexTile> _fixedTerritory = new();

//    private void Start()
//    {
//        _player = GetComponent<Player>();
//        _grid = FindObjectOfType<HexGridLegacy>();

//        if (_allHexes == null || _allHexes.Count == 0)
//            _allHexes = _grid.GetAllHexes().ToList();

//        GiveStartTerritory();
//    }

//    private void OnCollisionEnter(Collision collision)
//    {
//        if (!collision.transform.TryGetComponent(out HexTile hex))
//            return;

//        // ���� ���� �� ��� � ��������� � �������
//        if (hex.OwnerId != _player.Id)
//        {
//            if (_trail.Contains(hex))
//                return;

//            _trail.Add(hex);
//            hex.SetOwner(_player.Id, _player.Color); // ������ ��������� � ����� OnOwnerChanged �������� Stretch
//            // ����������� Stretch() ������� �� �����, �� ���� ��������� HexTileView

//            // ��������� �������� ��� ����������� bounce-�����
//            if (hex.TryGetComponent<IHexAnimationHandler>(out var anim))
//                _trailAnimators.Add(anim);
//        }
//        // ���� ����� �� ���� ���� � ���� ������������� trail � ��������� ����������
//        else if (_trail.Count > 0)
//        {
//            FixTerritory();
//            SequenceAnimator.AnimateWave(_trailAnimators, transform, waveSpeed: 2f, maxDelay: 0.5f);
//            _trail.Clear();
//            _trailAnimators.Clear();
//        }
//    }

//    private void GiveStartTerritory()
//    {
//        // ����������� ��������� � ���� ��� �������
//        var startHex = _player.StartHex; // ����� Player ������ ������
//        startHex.SetOwner(_player.Id, _player.Color);

//        _fixedTerritory.Add(startHex);
//        foreach (var nei in _grid.GetNeighbors(startHex))
//        {
//            nei.SetOwner(_player.Id, _player.Color);
//            _fixedTerritory.Add(nei);
//        }

//        Debug.Log($"StartHex?-{_player.StartHex == null},_fixedTerritory-{_fixedTerritory.Count}");
//    }

//    private void FixTerritory()
//    {
//        // 1) ��������� ��� _trail � ����������� � ������ ������
//        foreach (var h in _trail)
//        {
//            if (_fixedTerritory.Add(h))
//                h.SetOwner(_player.Id, _player.Color);
//        }

//        // 2) �������� ������ ���� ������, ������� ���� �������� � ��������� �������
//        var additional = new List<HexTile>(_trail);
//        var colliderGO = new GameObject("TrailCollider2D");
//        var poly = colliderGO.AddComponent<PolygonCollider2D>();

//        // ������ ������� �� XZ ��������
//        poly.points = _trail
//            .Select(h => (Vector2)(Vector3)h.transform.position)
//            .ToArray();

//        foreach (var hex in _allHexes)
//        {
//            if (_fixedTerritory.Contains(hex) || _trail.Contains(hex))
//                continue;

//            Vector2 p = (Vector2)(Vector3)hex.transform.position;
//            if (poly.OverlapPoint(p))
//            {
//                hex.SetOwner(_player.Id, _player.Color);
//                _fixedTerritory.Add(hex);
//                additional.Add(hex);
//            }
//        }

//        Destroy(colliderGO);

//        // 3) ��������� �� ��������� ��� bounce-�����
//        foreach (var h in additional)
//        {
//            if (h.TryGetComponent<IHexAnimationHandler>(out var anim))
//                _trailAnimators.Add(anim);
//        }
//    }
//}
