using System.Collections.Generic;
using System.IO;
using UnityEditor;

/// Ищет blender.exe. Путь машинно-зависимый, поэтому живёт в EditorPrefs,
/// а не в ассете — иначе он уехал бы в репозиторий вместе с проектом.
public sealed class BlenderLocator
{
    private const string PreferenceKey = "51Percent.AnimationPipeline.BlenderPath";
    private const string ExecutableName = "blender.exe";

    private static readonly string[] ProbeRoots =
    {
        @"C:\Program Files\Blender Foundation",
        @"C:\Program Files (x86)\Steam\steamapps\common\Blender",
        @"C:\Program Files\Blender",
    };

    public string Resolve()
    {
        string saved = EditorPrefs.GetString(PreferenceKey, string.Empty);

        if (File.Exists(saved))
            return saved;

        foreach (string candidate in EnumerateCandidates())
            if (File.Exists(candidate))
                return candidate;

        return string.Empty;
    }

    public void Remember(string path)
    {
        EditorPrefs.SetString(PreferenceKey, path);
    }

    private IEnumerable<string> EnumerateCandidates()
    {
        foreach (string root in ProbeRoots)
        {
            if (!Directory.Exists(root))
                continue;

            yield return Path.Combine(root, ExecutableName);

            // У установок из Blender Foundation исполняемый файл лежит в папке версии
            foreach (string versioned in Directory.GetDirectories(root))
                yield return Path.Combine(versioned, ExecutableName);
        }
    }
}
