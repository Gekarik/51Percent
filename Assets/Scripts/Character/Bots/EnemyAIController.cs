using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Mover), typeof(Conquester), typeof(PathProvider))]
public class EnemyAIController : MonoBehaviour
{
    private enum State { Idle, Invade, Return, Dead }

    [SerializeField, Range(1, 20)] private int aggressionLevel = 5;
    [SerializeField] private float waitTime = 1f;

    private Conquester _conquester;
    private PathProvider _pathProvider;
    private IHexGridProvider _grid;
    private State _state = State.Idle;
    private List<Hex> _trailPath;

    private void Awake()
    {
        _conquester = GetComponent<Conquester>();
        _pathProvider = GetComponent<PathProvider>();
        //_conquester.TrailInterrupted += OnTrailInterrupted;
    }

    private void OnDestroy()
    {
        //_conquester.TrailInterrupted -= OnTrailInterrupted;
    }

    public void Init(IHexGridProvider gridProvider)
    {
        _grid = gridProvider;
        StartCoroutine(StateMachine());
    }

    private IEnumerator StateMachine()
    {
        while (_state != State.Dead)
        {
            switch (_state)
            {
                case State.Idle:
                    yield return new WaitForSeconds(waitTime);
                    _trailPath = new TrailPlanner().BuildTrail(_conquester.FixedHexes, _grid, aggressionLevel);
                    if (_trailPath != null && _trailPath.Count > 1)
                    {
                        _pathProvider.SetPath(_trailPath, _grid);
                        _state = State.Invade;
                    }
                    break;

                case State.Invade:
                    if (_pathProvider.IsDone)
                    {
                        var returnPath = new TrailPlanner().BuildReturn(_trailPath, _conquester.FixedHexes, _grid);
                        if (returnPath != null && returnPath.Count > 1)
                        {
                            _pathProvider.SetPath(returnPath, _grid);
                            _state = State.Return;
                        }
                        else
                        {
                            _state = State.Idle;
                        }
                    }
                    break;

                case State.Return:
                    if (_pathProvider.IsDone)
                    {
                        _conquester.FixHexes(_trailPath);
                        _trailPath = null;
                        _state = State.Idle;
                    }
                    break;

                case State.Dead:
                    yield break;
            }
            yield return null;
        }
    }

    private void OnTrailInterrupted(ICharacter owner, ICharacter interrupter)
    {
        if (interrupter == this.GetComponent<ICharacter>()) 
            return;
    }
}
