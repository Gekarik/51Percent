using UnityEditor;
using UnityEngine;

public sealed class StaticTailResolver
{
    private const float ValueEpsilon = 0.0001f;
    private const float UndeterminedTime = 0f;

    public float ResolveLastMeaningfulTime(AnimationClip clip)
    {
        float lastChange = UndeterminedTime;

        foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(clip))
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);

            if (curve != null)
                lastChange = Mathf.Max(lastChange, ResolveLastChange(curve));
        }

        return lastChange;
    }

    private float ResolveLastChange(AnimationCurve curve)
    {
        Keyframe[] keys = curve.keys;

        if (keys.Length == 0)
            return UndeterminedTime;

        float finalValue = keys[keys.Length - 1].value;

        for (int i = keys.Length - 1; i >= 0; i--)
            if (Mathf.Abs(keys[i].value - finalValue) > ValueEpsilon)
                return keys[i].time;

        return UndeterminedTime;
    }
}
