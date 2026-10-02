using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class SpawnAnimationProbe
{
    private const string SessionKey = "51Percent.VerifySpawn";
    private const float RespawnAt = 2f;
    private const float ReportAt = 5f;
    private const float PositionTolerance = 0.01f;
    private const string LandingStateName = "Falling To Landing";

    private static readonly Dictionary<int, Vector3> Anchors = new Dictionary<int, Vector3>();
    private static readonly HashSet<int> Landed = new HashSet<int>();

    private static bool _respawnRequested;
    private static int _respawnId;
    private static bool _failed;

    static SpawnAnimationProbe() => EditorApplication.update += Check;

    [MenuItem("Tools/51 Percent/Verify Spawn Animation")]
    private static void Run()
    {
        SessionState.SetBool(SessionKey, true);
        EditorApplication.isPlaying = true;
    }

    private static void Check()
    {
        if (!SessionState.GetBool(SessionKey, false) || !EditorApplication.isPlaying)
            return;

        foreach (var character in Object.FindObjectsOfType<CharacterBase>())
            Inspect(character);

        RequestRespawn();
        Report();
    }

    private static void Inspect(CharacterBase character)
    {
        if (character.State != CharacterState.Alive)
            return;

        int id = character.GetInstanceID();

        if (character.IsLanding)
            InspectLanding(character, id);
        else if (Anchors.ContainsKey(id) && character.GetComponent<Mover>().enabled)
            Landed.Add(id);
    }

    private static void InspectLanding(CharacterBase character, int id)
    {
        if (!Anchors.ContainsKey(id))
            Anchors[id] = character.transform.position;

        if (character.GetComponent<Mover>().enabled || character.GetComponent<Grabber>().enabled
            || character.GetComponent<Conqueror>().enabled)
            _failed = true;

        var animator = character.GetComponentInChildren<Animator>();

        if (!animator.GetBool(AnimatorParams.IsLanding))
            _failed = true;

        if (!IsPlayingLanding(animator))
            _failed = true;

        if ((character.transform.position - Anchors[id]).sqrMagnitude > PositionTolerance)
            _failed = true;
    }

    private static bool IsPlayingLanding(Animator animator)
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName(LandingStateName)
            || animator.GetNextAnimatorStateInfo(0).IsName(LandingStateName);
    }

    private static void RequestRespawn()
    {
        if (_respawnRequested || Time.time < RespawnAt)
            return;

        var spawner = Object.FindObjectOfType<PlayerSpawner>();

        if (spawner == null)
            return;

        _respawnId = spawner.Respawn().GetInstanceID();
        _respawnRequested = true;
    }

    private static void Report()
    {
        if (Time.time < ReportAt)
            return;

        SessionState.SetBool(SessionKey, false);
        Debug.Log($"SPAWN_CHECK: observed={Anchors.Count}, completed={Landed.Count}, "
            + $"respawnCompleted={Landed.Contains(_respawnId)}, failed={_failed}");
        EditorApplication.isPlaying = false;
        EditorApplication.isPaused = false;
    }
}
