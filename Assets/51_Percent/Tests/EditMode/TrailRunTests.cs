using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class TrailRunTests
{
    private FakeGrid _grid;
    private FakeTerritory _territory;
    private FakeCharacter _owner;
    private FakeCharacter _rival;
    private CharacterStats _stats;
    private TrailRun _run;

    [SetUp]
    public void SetUp()
    {
        _grid = new FakeGrid(width: 5, height: 5);
        _territory = new FakeTerritory();
        _owner = new FakeCharacter();
        _rival = new FakeCharacter();
        _stats = new CharacterStats();
        _stats.SetBase(StatType.CaptureWidth, 1f);
        _run = new TrailRun(_territory, _grid, _owner, _stats);
    }

    [Test]
    public void EnteringFreeCell_AddsItToTrail()
    {
        _run.Enter(_grid.At(1, 1));

        Assert.That(_run.Hexes, Is.EqualTo(new[] { _grid.At(1, 1) }));
        Assert.That(_grid.At(1, 1).State, Is.EqualTo(HexState.PartOfTrail));
    }

    [Test]
    public void EnteringForeignTrail_ReportsInterruption()
    {
        var foreign = _grid.At(2, 2);
        _territory.TrailHex(_rival, foreign);
        ICharacter reportedOwner = null;
        ICharacter reportedStepper = null;
        _run.TrailInterrupted += (trailOwner, stepper) =>
        {
            reportedOwner = trailOwner;
            reportedStepper = stepper;
        };

        _run.Enter(foreign);

        Assert.That(reportedOwner, Is.SameAs(_rival));
        Assert.That(reportedStepper, Is.SameAs(_owner));
    }

    [Test]
    public void ReturningToOwnTerritory_ClosesTrailWithCapture()
    {
        var home = _grid.At(0, 0);
        _territory.FixHex(_owner, home);
        _run.Enter(_grid.At(1, 0));
        _run.Enter(_grid.At(2, 0));
        IReadOnlyList<IHex> captured = null;
        _run.AreaCaptured += (owner, area) => captured = area;

        _run.Enter(home);

        Assert.That(_territory.CaptureCalls, Is.EqualTo(1), "Возврат домой не закрыл трейл захватом.");
        Assert.That(captured, Is.Not.Null);
        Assert.That(_run.Hexes, Is.Empty, "Трейл не очищен после захвата.");
    }

    [Test]
    public void WideCapture_AddsNeighboursOfEnteredCell()
    {
        _stats.SetBase(StatType.CaptureWidth, 2f);

        _run.Enter(_grid.At(2, 2));

        Assert.That(_run.Hexes, Has.Member(_grid.At(2, 2)));
        Assert.That(_run.Hexes.Count, Is.GreaterThan(1), "Широкий захват не добавил соседние клетки.");
    }

    [Test]
    public void Abandon_ReturnsTrailCellsToPreviousOwners()
    {
        var rivalCell = _grid.At(3, 3);
        _territory.FixHex(_rival, rivalCell);
        _run.Enter(rivalCell);
        Assert.That(_territory.OwnerOf(rivalCell), Is.SameAs(_owner), "Клетка не перешла в трейл.");

        _run.Abandon();

        Assert.That(_territory.OwnerOf(rivalCell), Is.SameAs(_rival), "Клетка не вернулась прежнему владельцу.");
        Assert.That(_run.Hexes, Is.Empty);
        Assert.That(_run.CurrentHex, Is.Null);
    }

    private sealed class FakeTerritory : ITerritoryWriter
    {
        private readonly Dictionary<IHex, ICharacter> _owners = new Dictionary<IHex, ICharacter>();

        public int CaptureCalls { get; private set; }

        public ICharacter OwnerOf(IHex hex) => _owners.TryGetValue(hex, out var owner) ? owner : null;

        public IDisposable BeginChanges() => new Scope();

        public IReadOnlyCollection<IHex> GetFixedByOwner(ICharacter character)
        {
            return _owners.Where(pair => pair.Value == character && pair.Key.State == HexState.Busy)
                .Select(pair => pair.Key).ToArray();
        }

        public bool IsFixedBy(ICharacter character, IHex hex)
        {
            return OwnerOf(hex) == character && hex.State == HexState.Busy;
        }

        public void FixHex(ICharacter character, IHex hex)
        {
            _owners[hex] = character;
            hex.SetOwner(character, HexState.Busy);
        }

        public void ReleaseHex(IHex hex)
        {
            _owners.Remove(hex);
            hex.SetOwner(null, HexState.Empty);
        }

        public void TrailHex(ICharacter character, IHex hex)
        {
            _owners[hex] = character;
            hex.SetOwner(character, HexState.PartOfTrail);
        }

        public IReadOnlyList<IHex> CaptureEnclosedArea(ICharacter owner, IReadOnlyCollection<IHex> trail)
        {
            CaptureCalls++;
            foreach (var hex in trail.ToArray())
                FixHex(owner, hex);

            return trail.ToArray();
        }

        public void ReleaseDisconnectedFragments(ICharacter character) { }

        public void OnCharacterDied(ICharacter character) { }

        private sealed class Scope : IDisposable
        {
            public void Dispose() { }
        }
    }

    private sealed class FakeGrid : IHexGridProvider
    {
        private readonly Dictionary<HexCoord, IHex> _cells = new Dictionary<HexCoord, IHex>();

        public FakeGrid(int width, int height)
        {
            for (int q = 0; q < width; q++)
                for (int r = 0; r < height; r++)
                    _cells[new HexCoord(q, r)] = new FakeHex(new HexCoord(q, r));

            AllHexes = _cells.Values.ToArray();
        }

        public int Count => _cells.Count;
        public IReadOnlyList<IHex> AllHexes { get; }

        public IHex At(int q, int r) => _cells[new HexCoord(q, r)];

        public IHex GetHex(HexCoord coord) => _cells.TryGetValue(coord, out var hex) ? hex : null;

        public bool TryGetHex(HexCoord coord, out IHex hex) => _cells.TryGetValue(coord, out hex);

        public IHex GetHexAt(Vector3 worldPosition) => null;

        public HexCoord GetCoord(IHex hex) => hex.Coord;

        public IEnumerable<IHex> GetNeighbors(HexCoord coord)
        {
            foreach (var offset in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
                if (_cells.TryGetValue(new HexCoord(coord.Q + offset.Item1, coord.R + offset.Item2), out var hex))
                    yield return hex;
        }

        public IEnumerable<IHex> GetNeighbors(IHex hex) => GetNeighbors(hex.Coord);

        public int CalculateDistance(HexCoord a, HexCoord b) => Mathf.Abs(a.Q - b.Q) + Mathf.Abs(a.R - b.R);

        public int CalculateDistance(IHex a, IHex b) => CalculateDistance(a.Coord, b.Coord);

        public IHex GetRandomHex() => AllHexes[0];

        public IEnumerable<IHex> GetHexesInRadius(HexCoord center, int radius)
        {
            foreach (var pair in _cells)
                if (CalculateDistance(center, pair.Key) <= radius)
                    yield return pair.Value;
        }
    }

    private sealed class FakeHex : IHex
    {
        public FakeHex(HexCoord coord) => Coord = coord;

        public Transform Transform => null;
        public HexCoord Coord { get; }
        public HexState State { get; private set; } = HexState.Empty;
        public ICharacter Owner { get; private set; }
        public Transform ViewTransform => null;

        public event Action<IHex> StateChanged;

        public void SetOwner(ICharacter owner, HexState state)
        {
            Owner = owner;
            State = state;
            StateChanged?.Invoke(this);
        }

        public void Reset() => SetOwner(null, HexState.Empty);
    }

    private sealed class FakeCharacter : ICharacter
    {
        public string Name => "fake";
        public bool IsHuman => false;
        public bool CanAct { get; set; } = true;
        public PlayerStats LifeStats { get; } = new PlayerStats();
        public CharacterState State { get; set; } = CharacterState.Alive;
        public Color Color => Color.white;
        public Transform Transform => null;
        public IBoosterObservable BoosterObservable => null;
        public ITrailVisualProvider TrailVisual => null;
        public ITrailObservable Trail => null;

        public Transform GetSocket(SocketType socket) => null;
        public void Kill() { }
        public bool TryDie() => true;
    }
}
