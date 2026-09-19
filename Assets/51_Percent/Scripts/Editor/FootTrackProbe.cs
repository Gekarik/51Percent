using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

// Диагностика стойки: где ходят ступни относительно корня модели за время клипа.
// Нужна, чтобы поставить доску под опорную ногу, а не под осевую линию,
// иначе толчковая нога упирается в доску вместо земли
public static class FootTrackProbe
{
    private const string CharacterPath = "Assets/51_Percent/Prefabs/Characters/Player.prefab";
    private const string ClipPath = "Assets/51_Percent/Animations/Skateboarding_InPlace.anim";
    private const string LeftFootName = "foot_l";
    private const string RightFootName = "foot_r";
    private const int Samples = 8;

    [MenuItem("Tools/51 Percent/Probe Foot Track")]
    private static void Probe()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CharacterPath);
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(ClipPath);

        if (prefab == null || clip == null)
        {
            Debug.LogError("Не найден префаб персонажа или клип.");
            return;
        }

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

        try
        {
            Report(instance, clip);
        }
        finally
        {
            if (AnimationMode.InAnimationMode())
                AnimationMode.StopAnimationMode();

            Object.DestroyImmediate(instance);
        }
    }

    private static void Report(GameObject instance, AnimationClip clip)
    {
        var model = instance.GetComponentInChildren<CharacterView>().transform;
        var left = Find(model, LeftFootName);
        var right = Find(model, RightFootName);

        if (left == null || right == null)
        {
            Debug.LogError($"Не найдены кости '{LeftFootName}' / '{RightFootName}'.");
            return;
        }

        var report = new StringBuilder();
        report.Append($"FOOT_TRACK: {clip.name}, длина {clip.length:F3}");

        AnimationMode.StartAnimationMode();

        for (int i = 0; i < Samples; i++)
        {
            float time = clip.length * i / Samples;

            AnimationMode.BeginSampling();
            AnimationMode.SampleAnimationClip(model.gameObject, clip, time);
            AnimationMode.EndSampling();

            Vector3 l = model.InverseTransformPoint(left.position);
            Vector3 r = model.InverseTransformPoint(right.position);
            report.Append($" | t{i}: L({l.x:F3},{l.y:F3},{l.z:F3}) R({r.x:F3},{r.y:F3},{r.z:F3})");
        }

        Debug.Log(report.ToString());
    }

    private static Transform Find(Transform root, string name)
    {
        return root.GetComponentsInChildren<Transform>(true).FirstOrDefault(child => child.name == name);
    }
}
