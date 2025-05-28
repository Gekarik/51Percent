using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(ICharacter))]
public class Conquester : MonoBehaviour
{
    [SerializeField] private HexGrid _grid;

    //еу
    private HexEnvironmentAdapter _env;
    //

    public Action<ICharacter, ICharacter> TrailInterrupted;
    public Action<IReadOnlyList<Transform>> AreaCaptured;

    private ICharacter _player;
    private readonly List<Hex> _trailList = new List<Hex>();
    private readonly HashSet<Hex> _fixed = new HashSet<Hex>();

    private void Awake()
    {
        _player = GetComponent<ICharacter>();
        _grid = FindObjectOfType<HexGrid>();

        // получаем размер гекса по оси Z
        float hexSizeZ = _grid.AllHexes[0].GetRendererBounds().size.z;
        // порог — чуть больше диаметра (1.1f)
        float maxDist = hexSizeZ * 1.1f;
        _env = new HexEnvironmentAdapter(_grid);
    }

    private void OnEnable()
    {
        AreaCaptured += _grid.OnAreaCaptured;
    }

    private void OnDisable()
    {
        AreaCaptured -= _grid.OnAreaCaptured;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out Hex hex) == false)
            return;

        switch (hex.State)
        {
            case HexState.PartOfTrail when hex.Owner != _player:
                TrailInterrupted?.Invoke(hex.Owner, _player);
                break;

            case HexState.Busy when hex.Owner == _player && _trailList.Count > 0:
                HandleClosure(hex);
                break;

            case HexState.Empty:
                HandleTrail(hex);
                break;
        }
    }

    private void HandleTrail(Hex hex)
    {
        if (_trailList.Contains(hex) == false)
        {
            _trailList.Add(hex);
            hex.SetOwner(_player, HexState.PartOfTrail);
        }
    }

    private void HandleClosure(Hex returnHex)
    {
        // Перед вызовом будьте уверены, что returnHex находится в _trailList
        if (!_trailList.Contains(returnHex))
            _trailList.Add(returnHex);

        // Собираем состояние
        var state = new ConquestState(
            fixedHexes: new HashSet<Hex>(_fixed),
            trailHexes: _trailList);

        // Вычисляем область для захвата
        var toCapture = ConquestAlgorithm.ComputeCapturedArea(_env, state);

        // Захватываем и фиксируем новые гексы
        foreach (var hex in toCapture)
            CaptureHex(hex);

        // Анимация
        var transforms = toCapture
            .Where(h => h.HexView != null)
            .Select(h => h.HexView.transform)
            .Distinct()
            .ToList();
        AreaCaptured?.Invoke(transforms);

        // Сбрасываем трейл
        _trailList.Clear();
    }

    private void CaptureHex(Hex hex)
    {
        if (_fixed.Add(hex))
            hex.SetOwner(_player, HexState.Busy);
    }

    public void Init(HexGrid grid) 
    {
        _grid = grid ?? throw new ArgumentNullException();
    }

    public void GetStartTerritory(Hex startHex)
    {
        var startTerritory = _grid.GetNeighbors(startHex);

        foreach (var h in startTerritory)
            CaptureHex(h);
    }

    public void Reset()
    {
        foreach (var h in _fixed) 
            h.Reset();

        _fixed.Clear();
        _trailList.Clear();
    }
}
