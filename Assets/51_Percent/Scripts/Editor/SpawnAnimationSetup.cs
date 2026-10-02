using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class SpawnAnimationSetup
{
    private const string ControllerPath = "Assets/51_Percent/Animators/Character.controller";
    private const string SettingsPath = "Assets/51_Percent/ScriptableObjects/SpawnDescentSettings.asset";
    private const string LandingStateName = "Falling To Landing";
    private const float SpeedThreshold = 0.1f;
    private const float ExitDuration = 0.25f;

    [MenuItem("Tools/51 Percent/Configure Spawn Animation")]
    private static void Configure()
    {
        ConfigureController();
        ConfigurePrefabs();
        Debug.Log("Spawn setup complete: срок окна приземления в CharacterConfig, форма спуска в SpawnDescentSettings.");
    }

    private static void ConfigureController()
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        var machine = controller.layers[0].stateMachine;
        var landing = FindState(machine, LandingStateName);
        var idle = FindState(machine, "Idle");
        var skating = FindState(machine, "Skateboarding");

        AddParameter(controller, AnimatorParams.IsLanding, AnimatorControllerParameterType.Bool);
        AddParameter(controller, AnimatorParams.LocomotionMode, AnimatorControllerParameterType.Int);
        RemoveParameter(controller, "LandingProgress");

        // Стартовое состояние — Idle: окно приземления открывается из кода,
        // и до этого момента графу нечего проигрывать
        machine.defaultState = idle;

        // Клип идёт своим темпом, нормализованное время вручную не гонится
        landing.timeParameterActive = false;
        landing.timeParameter = string.Empty;

        ConfigureLandingTransitions(machine, landing, idle, skating);
        ConfigureSkatingExit(skating, idle);
        AlignSpeedThresholds(machine);

        EditorUtility.SetDirty(landing);
        EditorUtility.SetDirty(skating);
        EditorUtility.SetDirty(machine);
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
    }

    private static void ConfigureLandingTransitions(AnimatorStateMachine machine, AnimatorState landing,
        AnimatorState idle, AnimatorState skating)
    {
        foreach (var transition in machine.anyStateTransitions.ToArray())
        {
            if (transition.destinationState == landing)
                machine.RemoveAnyStateTransition(transition);
            else if (transition.destinationState == skating)
                ConfigureSkatingEntry(transition);
        }

        var enter = machine.AddAnyStateTransition(landing);
        enter.AddCondition(AnimatorConditionMode.If, 0f, AnimatorParams.IsLanding);
        enter.hasExitTime = false;
        enter.duration = 0f;
        enter.canTransitionToSelf = false;

        foreach (var transition in landing.transitions.ToArray())
            landing.RemoveTransition(transition);

        var exit = landing.AddTransition(idle);
        exit.AddCondition(AnimatorConditionMode.IfNot, 0f, AnimatorParams.IsLanding);
        exit.hasExitTime = false;
        exit.hasFixedDuration = true;
        exit.duration = ExitDuration;
    }

    private static void ConfigureSkatingEntry(AnimatorStateTransition transition)
    {
        transition.conditions = Array.Empty<AnimatorCondition>();
        transition.AddCondition(AnimatorConditionMode.Equals, (int)LocomotionMode.Skating, AnimatorParams.LocomotionMode);
        transition.AddCondition(AnimatorConditionMode.IfNot, 0f, AnimatorParams.IsLanding);
        transition.hasExitTime = false;
        transition.canTransitionToSelf = false;
        EditorUtility.SetDirty(transition);
    }

    private static void ConfigureSkatingExit(AnimatorState skating, AnimatorState idle)
    {
        foreach (var transition in skating.transitions.ToArray())
            skating.RemoveTransition(transition);

        var exit = skating.AddTransition(idle);
        exit.AddCondition(AnimatorConditionMode.NotEqual, (int)LocomotionMode.Skating, AnimatorParams.LocomotionMode);
        exit.hasExitTime = false;
        exit.duration = ExitDuration;
    }

    // Одинаковый порог исключает одновременные условия Idle -> Run и Run -> Idle
    private static void AlignSpeedThresholds(AnimatorStateMachine machine)
    {
        foreach (var child in machine.states)
            foreach (var transition in child.state.transitions)
            {
                var conditions = transition.conditions;
                for (int i = 0; i < conditions.Length; i++)
                    if (conditions[i].parameter == AnimatorParams.Speed)
                        conditions[i].threshold = SpeedThreshold;

                transition.conditions = conditions;
                EditorUtility.SetDirty(transition);
            }
    }

    private static void ConfigurePrefabs()
    {
        var settings = AssetDatabase.LoadAssetAtPath<SpawnDescentSettings>(SettingsPath);

        foreach (string name in new[] { "Player", "Enemy" })
        {
            string path = $"Assets/51_Percent/Prefabs/Characters/{name}.prefab";
            var root = PrefabUtility.LoadPrefabContents(path);

            try
            {
                AssignSettings(Require<SpawnDescentAnimator>(root), settings);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
    }

    private static T Require<T>(GameObject root) where T : Component
    {
        return root.GetComponent<T>() ?? root.AddComponent<T>();
    }

    private static void AssignSettings(SpawnDescentAnimator animator, SpawnDescentSettings settings)
    {
        var serialized = new SerializedObject(animator);
        serialized.FindProperty("_settings").objectReferenceValue = settings;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    // Unity дописывает суффикс к имени при дублировании состояния, поэтому ищем по началу имени:
    // иначе ручная правка графа молча ломает настройку
    private static AnimatorState FindState(AnimatorStateMachine machine, string name)
    {
        return machine.states.Select(child => child.state).Single(state => state.name.StartsWith(name));
    }

    private static void AddParameter(AnimatorController controller, string name, AnimatorControllerParameterType type)
    {
        if (!controller.parameters.Any(parameter => parameter.name == name))
            controller.AddParameter(name, type);
    }

    private static void RemoveParameter(AnimatorController controller, string name)
    {
        var parameter = controller.parameters.FirstOrDefault(item => item.name == name);

        if (parameter != null)
            controller.RemoveParameter(parameter);
    }
}
