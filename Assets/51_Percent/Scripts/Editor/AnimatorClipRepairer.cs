using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class AnimatorClipRepairer
{
    private const string ControllerPath = "Assets/51_Percent/Animators/Character.controller";
    private const string PreviewPrefix = "__preview__";

    private readonly struct RepairTarget
    {
        public string StateName { get; }
        public string SourcePath { get; }
        public bool InPlace { get; }
        public bool Loop { get; }

        public RepairTarget(string stateName, string sourcePath, bool inPlace, bool loop)
        {
            StateName = stateName;
            SourcePath = sourcePath;
            InPlace = inPlace;
            Loop = loop;
        }
    }

    private static readonly RepairTarget[] Targets =
    {
        new RepairTarget("Skateboarding", "Assets/51_Percent/Ch46_nonPBR@Skateboarding(1).fbx",
            inPlace: true, loop: true),
    };

    [MenuItem("Tools/51 Percent/Repair Animator Clips")]
    private static void Repair()
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);

        foreach (var target in Targets)
            RepairState(controller, target);

        AssetDatabase.SaveAssets();
    }

    private static void RepairState(AnimatorController controller, RepairTarget target)
    {
        if (!EnsureHumanoid(target.SourcePath))
            return;

        if (target.InPlace || target.Loop)
            EnsureClipSettings(target);

        var clip = LoadHumanoidClip(target.SourcePath);

        if (clip == null)
            return;

        var state = controller.layers
            .SelectMany(layer => layer.stateMachine.states)
            .Select(child => child.state)
            .FirstOrDefault(candidate => candidate.name.StartsWith(target.StateName));

        if (state == null)
        {
            Debug.LogError($"Состояние '{target.StateName}' не найдено в {ControllerPath}.");
            return;
        }

        state.motion = clip;
        EditorUtility.SetDirty(state);
        EditorUtility.SetDirty(controller);
        Debug.Log($"Состояние '{state.name}' переведено на humanoid-клип {clip.name}, длина {clip.length:F3} с.");
    }

    private static bool EnsureHumanoid(string path)
    {
        if (!(AssetImporter.GetAtPath(path) is ModelImporter importer))
        {
            Debug.LogError($"{path} не найден или не является моделью.");
            return false;
        }

        if (importer.animationType == ModelImporterAnimationType.Human)
            return true;

        importer.animationType = ModelImporterAnimationType.Human;
        importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        importer.SaveAndReimport();
        Debug.Log($"{path} переимпортирован как Humanoid.");
        return true;
    }

    private static void EnsureClipSettings(RepairTarget target)
    {
        string path = target.SourcePath;

        if (!(AssetImporter.GetAtPath(path) is ModelImporter importer))
            return;

        var clips = importer.clipAnimations.Length > 0
            ? importer.clipAnimations
            : importer.defaultClipAnimations;

        if (clips.Length == 0)
        {
            Debug.LogError($"В {path} нет клипов, настраивать нечего.");
            return;
        }

        bool changed = false;

        foreach (var clip in clips)
        {
            if (target.InPlace && !IsInPlace(clip))
            {
                clip.lockRootPositionXZ = true;
                clip.keepOriginalPositionXZ = false;
                clip.lockRootRotation = true;
                clip.keepOriginalOrientation = false;
                clip.lockRootHeightY = true;
                clip.keepOriginalPositionY = true;
                changed = true;
            }

            if (target.Loop && !clip.loopTime)
            {
                clip.loopTime = true;
                changed = true;
            }
        }

        if (!changed)
            return;

        importer.clipAnimations = clips;
        importer.SaveAndReimport();
        Debug.Log($"{path}: клипы настроены (in-place: {target.InPlace}, зацикливание: {target.Loop}).");
    }

    private static bool IsInPlace(ModelImporterClipAnimation clip)
    {
        return clip.lockRootPositionXZ && !clip.keepOriginalPositionXZ
            && clip.lockRootRotation && !clip.keepOriginalOrientation
            && clip.lockRootHeightY;
    }

    private static AnimationClip LoadHumanoidClip(string path)
    {
        var clips = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>()
            .Where(clip => !clip.name.StartsWith(PreviewPrefix))
            .ToArray();

        if (clips.Length == 0)
        {
            Debug.LogError($"В {path} нет клипов.");
            return null;
        }

        var humanoid = clips.FirstOrDefault(clip => clip.isHumanMotion);

        if (humanoid == null)
            Debug.LogError($"Клип в {path} не стал humanoid. Проверь, что у модели корректный скелет.");

        return humanoid;
    }
}
