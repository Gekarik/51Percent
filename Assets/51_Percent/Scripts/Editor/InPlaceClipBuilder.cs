using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// Делает in-place копию клипа, у которого нет in-place версии в источнике.
// Настройки импорта тут бессильны: Bake Into Pose не удаляет проезд, а лишь
// перестаёт отдавать его корню, и тело всё равно уезжает. Убираем сам проезд —
// горизонтальные корневые кривые сплющиваются в константу
public static class InPlaceClipBuilder
{
    private const string SourcePath = "Assets/51_Percent/Ch46_nonPBR@Skateboarding(1).fbx";
    private const string OutputPath = "Assets/51_Percent/Animations/Skateboarding_InPlace.anim";
    private const string ControllerPath = "Assets/51_Percent/Animators/Character.controller";
    private const string StateName = "Skateboarding";
    private const string PreviewPrefix = "__preview__";

    // Вертикаль не трогаем: это присед и покачивание на доске, они нужны
    private static readonly string[] FlattenedCurves = { "RootT.x", "RootT.z" };

    [MenuItem("Tools/51 Percent/Build In-Place Skating Clip")]
    private static void Build()
    {
        var source = LoadSourceClip();

        if (source == null)
            return;

        var copy = Object.Instantiate(source);
        copy.name = "Skateboarding_InPlace";

        int flattened = FlattenHorizontalRoot(copy);
        MakeLooping(copy);

        AssetDatabase.DeleteAsset(OutputPath);
        AssetDatabase.CreateAsset(copy, OutputPath);
        AssetDatabase.SaveAssets();

        AssignToState(copy);
        Debug.Log($"In-place клип собран: {OutputPath}, сплющено кривых {flattened}, humanoid {copy.isHumanMotion}.");
    }

    private static AnimationClip LoadSourceClip()
    {
        var clip = AssetDatabase.LoadAllAssetsAtPath(SourcePath).OfType<AnimationClip>()
            .FirstOrDefault(candidate => !candidate.name.StartsWith(PreviewPrefix));

        if (clip == null)
            Debug.LogError($"В {SourcePath} нет клипов.");

        return clip;
    }

    // Значение берём на нулевом кадре: персонаж остаётся там, где стартовал
    private static int FlattenHorizontalRoot(AnimationClip clip)
    {
        var bindings = AnimationUtility.GetCurveBindings(clip);
        int flattened = 0;

        foreach (string propertyName in FlattenedCurves)
        {
            var binding = bindings.FirstOrDefault(candidate => candidate.propertyName == propertyName);

            if (string.IsNullOrEmpty(binding.propertyName))
                continue;

            var curve = AnimationUtility.GetEditorCurve(clip, binding);

            if (curve == null || curve.length == 0)
                continue;

            float start = curve.Evaluate(0f);
            AnimationUtility.SetEditorCurve(clip, binding, AnimationCurve.Constant(0f, clip.length, start));
            flattened++;
        }

        return flattened;
    }

    private static void MakeLooping(AnimationClip clip)
    {
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
    }

    private static void AssignToState(AnimationClip clip)
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        var state = controller.layers
            .SelectMany(layer => layer.stateMachine.states)
            .Select(child => child.state)
            .FirstOrDefault(candidate => candidate.name.StartsWith(StateName));

        if (state == null)
        {
            Debug.LogError($"Состояние '{StateName}' не найдено.");
            return;
        }

        state.motion = clip;
        EditorUtility.SetDirty(state);
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
    }
}
