using System.Linq;
using UnityEditor.Animations;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class ClipMotionProbe
{
    private const string ControllerPath = "Assets/51_Percent/Animators/Character.controller";
    private const string StateName = "Skateboarding";
    private const int Samples = 12;

    [MenuItem("Tools/51 Percent/Probe Clip Motion")]
    private static void Probe()
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        var state = controller.layers
            .SelectMany(layer => layer.stateMachine.states)
            .Select(child => child.state)
            .FirstOrDefault(candidate => candidate.name.StartsWith(StateName));

        var clip = state == null ? null : state.motion as AnimationClip;

        if (clip == null)
        {
            Debug.LogError($"На состоянии '{StateName}' нет клипа.");
            return;
        }

        var report = new StringBuilder();
        report.Append($"CLIP_PROBE: {clip.name}, длина {clip.length:F3} с, humanoid {clip.isHumanMotion}");

        var bindings = AnimationUtility.GetCurveBindings(clip);
        report.Append($" | кривых: {bindings.Length}");

        foreach (string name in new[] { "RootT.x", "RootT.y", "RootT.z" })
            report.Append(" | " + DescribeCurve(clip, bindings, name));

        report.Append($" | averageSpeed: {clip.averageSpeed}");
        report.Append($" | apparentSpeed: {clip.apparentSpeed:F3}");
        report.Append($" | rootCurves: {clip.hasRootCurves}, motionCurves: {clip.hasMotionCurves}");

        var rootNames = bindings.Where(b => b.propertyName.StartsWith("Root") || b.propertyName.StartsWith("Motion"))
            .Select(b => b.propertyName).Distinct().ToArray();
        report.Append(" | корневые: " + (rootNames.Length == 0 ? "нет" : string.Join(",", rootNames)));

        Debug.Log(report.ToString());
    }

    private static string DescribeCurve(AnimationClip clip, EditorCurveBinding[] bindings, string propertyName)
    {
        var binding = bindings.FirstOrDefault(b => b.propertyName == propertyName);

        if (string.IsNullOrEmpty(binding.propertyName))
            return $"{propertyName}: кривой нет";

        var curve = AnimationUtility.GetEditorCurve(clip, binding);

        if (curve == null || curve.length == 0)
            return $"{propertyName}: пусто";

        float min = float.MaxValue;
        float max = float.MinValue;

        for (int i = 0; i <= Samples; i++)
        {
            float value = curve.Evaluate(clip.length * i / Samples);
            min = Mathf.Min(min, value);
            max = Mathf.Max(max, value);
        }

        return $"{propertyName}: от {min:F3} до {max:F3}, размах {max - min:F3}, ключей {curve.length}";
    }
}
