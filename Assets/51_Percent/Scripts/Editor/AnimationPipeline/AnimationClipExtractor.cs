using UnityEditor;
using UnityEngine;

/// Выносит клип из модели в самостоятельный ассет.
/// Отдельный клип переживает переимпорт модели и не зависит от её настроек.
public sealed class AnimationClipExtractor
{
    private const string PreviewPrefix = "__preview__";

    public AnimationClip Extract(string modelPath, string clipPath, string clipName)
    {
        AnimationClip source = FindClip(modelPath);

        if (source == null)
            return null;

        var copy = Object.Instantiate(source);
        copy.name = clipName;
        AnimationUtility.SetAnimationClipSettings(copy, AnimationUtility.GetAnimationClipSettings(source));

        AssetDatabase.CreateAsset(copy, clipPath);
        AssetDatabase.SaveAssets();

        return copy;
    }

    public AnimationClip FindClip(string modelPath)
    {
        foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(modelPath))
            if (asset is AnimationClip clip && !clip.name.StartsWith(PreviewPrefix))
                return clip;

        return null;
    }
}
