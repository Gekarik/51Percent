using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class Conquester : MonoBehaviour
{
    [Header("Game Objects")]
    [SerializeField]
    private List<Hex> _allHexes;

    private HexGrid _grid;
    private Player _player;

    private List<Hex> _trail = new List<Hex>();
    private List<IHexAnimationHandler> _trailAnimators = new List<IHexAnimationHandler>();
    private HashSet<Hex> _fixedTerritory = new HashSet<Hex>();

    private void Start()
    {
        _player = GetComponent<Player>();
        _grid = FindObjectOfType<HexGrid>();

        if (_allHexes == null || _allHexes.Count == 0)
            _allHexes = _grid.GetAllHexes().ToList();

        GiveStartTerritory();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.TryGetComponent(out Hex hex))
        {
            if (hex.OwnerId != _player.Id)
            {
                if (!_trail.Contains(hex))
                {
                    _trail.Add(hex);
                    if (hex.TryGetComponent<IHexAnimationHandler>(out var animator))
                    {
                        _trailAnimators.Add(animator);
                        animator.Stretch();
                    }
                    hex.SetColor(_player.Color);
                }
            }
            else if (_trail.Count > 0)
            {
                FixTerritory();
                //HexSequenceAnimator.AnimateBounceSequentially(_trailAnimators, 0.1f);
                HexSequenceAnimator.AnimateWave(_trailAnimators, transform, waveSpeed: 2f, maxDelay: 2f);
                _trail.Clear();
                _trailAnimators.Clear();
            }
        }
    }

    private void GiveStartTerritory()
    {
        var start = _allHexes.Find(h => h == _player._startHex);
        start.SetOwner(_player.Id, _player.Color);
        _fixedTerritory.Add(start);

        foreach (var nei in _grid.GetNeighbors(start))
        {
            nei.SetOwner(_player.Id, _player.Color);
            _fixedTerritory.Add(nei);
        }
    }

    private void FixTerritory()
    {
        // —обираем список гексов дл€ анимации волны
        var hexesToAnimate = new List<Hex>(_trail);

        foreach (var h in _trail)
        {
            h.SetOwner(_player.Id, _player.Color);
            _fixedTerritory.Add(h);
        }

        var go = new GameObject("TrailCollider");
        var poly = go.AddComponent<PolygonCollider2D>();

        Vector2[] points = _trail
            .Select(h => (Vector2)new Vector2(h.transform.position.x, h.transform.position.z))
            .ToArray();
        poly.points = points;

        foreach (var hex in _allHexes)
        {
            if (_fixedTerritory.Contains(hex) || _trail.Contains(hex))
                continue;

            var pt = (Vector2)new Vector2(hex.transform.position.x, hex.transform.position.z);
            if (poly.OverlapPoint(pt))
            {
                hex.SetOwner(_player.Id, _player.Color);
                _fixedTerritory.Add(hex);
                hexesToAnimate.Add(hex); // ƒобавл€ем в список дл€ анимации
            }
        }

        Destroy(go);

        // «апуск волны дл€ всех гексов из _trail и дополнительно захваченных
        var animators = hexesToAnimate
            .Where(h => h.TryGetComponent<IHexAnimationHandler>(out _))
            .Select(h => h.GetComponent<IHexAnimationHandler>());

        HexSequenceAnimator.AnimateWave(animators, transform, waveSpeed: 2f, maxDelay: 0.5f);
    }
}