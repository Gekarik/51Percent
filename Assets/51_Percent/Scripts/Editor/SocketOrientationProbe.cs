using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class SocketOrientationProbe
{
    private const string CharacterPath = "Assets/51_Percent/Prefabs/Characters/Player.prefab";

    private static readonly string[] Sockets = { "BackSocket", "FeetSocket", "CrownSocket" };

    [MenuItem("Tools/51 Percent/Probe Socket Orientation")]
    private static void Probe()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CharacterPath);

        if (prefab == null)
        {
            Debug.LogError($"Не найден {CharacterPath}.");
            return;
        }

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

        try
        {
            var model = instance.GetComponentInChildren<CharacterView>().transform;
            var report = new StringBuilder("SOCKET_ORIENT:");

            foreach (string name in Sockets)
            {
                var socket = Find(model, name);

                if (socket == null)
                {
                    report.Append($" | {name}: не найден");
                    continue;
                }

                Vector3 right = model.InverseTransformDirection(socket.right);
                Vector3 up = model.InverseTransformDirection(socket.up);
                Vector3 forward = model.InverseTransformDirection(socket.forward);
                Vector3 euler = (Quaternion.Inverse(model.rotation) * socket.rotation).eulerAngles;

                report.Append($" | {name}: углы({euler.x:F0},{euler.y:F0},{euler.z:F0})");
                report.Append($" X→{Describe(right)} Y→{Describe(up)} Z→{Describe(forward)}");
            }

            Debug.Log(report.ToString());
        }
        finally
        {
            Object.DestroyImmediate(instance);
        }
    }

    private static string Describe(Vector3 direction)
    {
        var axes = new (string Name, Vector3 Value)[]
        {
            ("вправо", Vector3.right), ("влево", Vector3.left),
            ("вверх", Vector3.up), ("вниз", Vector3.down),
            ("вперёд", Vector3.forward), ("назад", Vector3.back),
        };

        var best = axes.OrderByDescending(axis => Vector3.Dot(direction, axis.Value)).First();
        return best.Name;
    }

    private static Transform Find(Transform root, string name)
    {
        return root.GetComponentsInChildren<Transform>(true).FirstOrDefault(child => child.name == name);
    }
}
