using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Mover), typeof(Conquester), typeof(PathProvider))]
public class EnemyAIController : MonoBehaviour
{
    private enum State { Idle, Invade, Return, Dead }

    [SerializeField, Range(1, 20)] private int aggressionLevel = 5;
    [SerializeField] private float waitTime = 1f;

    private Conquester conquester;
    private PathProvider pathProvider;
    private IHexGridProvider grid;
    private State state = State.Idle;
    private List<Hex> trailPath;

    private TrailPlanner planner = new TrailPlanner();

    private void Awake()
    {
        conquester = GetComponent<Conquester>();
        pathProvider = GetComponent<PathProvider>();
    }

    public void Init(IHexGridProvider gridProvider)
    {
        grid = gridProvider;
        StartCoroutine(StateMachine());
    }

    private IEnumerator StateMachine()
    {
        while (state != State.Dead)
        {
            switch (state)
            {
                case State.Idle:
                    yield return new WaitForSeconds(waitTime);
                    trailPath = planner.BuildTrail(conquester.FixedHexes, grid, aggressionLevel);
                    if (trailPath != null && trailPath.Count > 1)
                    {
                        Debug.Log($"[EnemyAI] Planned trail length: {trailPath.Count}");
                        pathProvider.SetPath(trailPath, grid);
                        state = State.Invade;
                    }
                    else
                        Debug.Log("[EnemyAI] No valid trail planned");
                    break;

                case State.Invade:
                    if (pathProvider.IsDone)
                    {
                        Debug.Log("[EnemyAI] Forward complete");
                        var returnPath = planner.BuildReturn(trailPath, conquester.FixedHexes, grid);
                        if (returnPath != null && returnPath.Count > 1)
                        {
                            Debug.Log($"[EnemyAI] Planned return length: {returnPath.Count}");
                            pathProvider.SetPath(returnPath, grid);
                            state = State.Return;
                        }
                        else
                        {
                            Debug.Log("[EnemyAI] No return path, idle");
                            state = State.Idle;
                        }
                    }
                    break;

                case State.Return:
                    if (pathProvider.IsDone)
                    {
                        Debug.Log("[EnemyAI] Return complete, capturing");
                        conquester.FixHexes(trailPath);
                        trailPath = null;
                        state = State.Idle;
                    }
                    break;
            }
            yield return null;
        }
    }
}