#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class BotPersonalityCreator
{
    private const string BasePath = "Assets/51_Percent/ScriptableObjects/BotPersonalities/";

    [MenuItem("51_Percent/Create Bot Personality Presets")]
    public static void CreateAllPresets()
    {
        EnsureDirectoryExists();

        CreateAggressiveHunter();
        CreateGreedyCollector();
        CreateTerritoryBuilder();
        CreateBalanced();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Bot Personality presets created successfully!");
    }

    private static void EnsureDirectoryExists()
    {
        if (!AssetDatabase.IsValidFolder("Assets/51_Percent"))
            AssetDatabase.CreateFolder("Assets", "51_Percent");

        if (!AssetDatabase.IsValidFolder("Assets/51_Percent/ScriptableObjects"))
            AssetDatabase.CreateFolder("Assets/51_Percent", "ScriptableObjects");

        if (!AssetDatabase.IsValidFolder("Assets/51_Percent/ScriptableObjects/BotPersonalities"))
            AssetDatabase.CreateFolder("Assets/51_Percent/ScriptableObjects", "BotPersonalities");
    }

    private static void CreateAggressiveHunter()
    {
        var preset = ScriptableObject.CreateInstance<BotPersonalitySettings>();

        SetPrivateField(preset, "_aggression", 0.8f);
        SetPrivateField(preset, "_riskTolerance", 0.6f);
        SetPrivateField(preset, "_greed", 0.2f);
        SetPrivateField(preset, "_territorialFocus", 0.3f);
        SetPrivateField(preset, "_reactionTime", 0.3f);
        SetPrivateField(preset, "_randomness", 0.15f);
        SetPrivateField(preset, "_detectionRadius", 10f);
        SetPrivateField(preset, "_maxTrailLength", 8);

        SaveAsset(preset, "AggressiveHunter");
    }

    private static void CreateGreedyCollector()
    {
        var preset = ScriptableObject.CreateInstance<BotPersonalitySettings>();

        SetPrivateField(preset, "_aggression", 0.2f);
        SetPrivateField(preset, "_riskTolerance", 0.3f);
        SetPrivateField(preset, "_greed", 0.9f);
        SetPrivateField(preset, "_territorialFocus", 0.4f);
        SetPrivateField(preset, "_reactionTime", 0.5f);
        SetPrivateField(preset, "_randomness", 0.2f);
        SetPrivateField(preset, "_detectionRadius", 12f);
        SetPrivateField(preset, "_maxTrailLength", 6);

        SaveAsset(preset, "GreedyCollector");
    }

    private static void CreateTerritoryBuilder()
    {
        var preset = ScriptableObject.CreateInstance<BotPersonalitySettings>();

        SetPrivateField(preset, "_aggression", 0.4f);
        SetPrivateField(preset, "_riskTolerance", 0.7f);
        SetPrivateField(preset, "_greed", 0.3f);
        SetPrivateField(preset, "_territorialFocus", 0.9f);
        SetPrivateField(preset, "_reactionTime", 0.4f);
        SetPrivateField(preset, "_randomness", 0.1f);
        SetPrivateField(preset, "_detectionRadius", 8f);
        SetPrivateField(preset, "_maxTrailLength", 15);

        SaveAsset(preset, "TerritoryBuilder");
    }

    private static void CreateBalanced()
    {
        var preset = ScriptableObject.CreateInstance<BotPersonalitySettings>();

        SetPrivateField(preset, "_aggression", 0.5f);
        SetPrivateField(preset, "_riskTolerance", 0.5f);
        SetPrivateField(preset, "_greed", 0.5f);
        SetPrivateField(preset, "_territorialFocus", 0.5f);
        SetPrivateField(preset, "_reactionTime", 0.4f);
        SetPrivateField(preset, "_randomness", 0.2f);
        SetPrivateField(preset, "_detectionRadius", 8f);
        SetPrivateField(preset, "_maxTrailLength", 10);

        SaveAsset(preset, "Balanced");
    }

    private static void SetPrivateField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (field != null)
            field.SetValue(obj, value);
    }

    private static void SaveAsset(ScriptableObject asset, string name)
    {
        string path = BasePath + name + ".asset";

        var existing = AssetDatabase.LoadAssetAtPath<BotPersonalitySettings>(path);
        if (existing != null)
        {
            Debug.Log($"Preset already exists: {name}");
            return;
        }

        AssetDatabase.CreateAsset(asset, path);
        Debug.Log($"Created preset: {name}");
    }
}
#endif
