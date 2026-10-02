using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class PlayModeGatekeeper
{
    static PlayModeGatekeeper()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.ExitingEditMode)
            return;

        var violations = ReferenceValidator.FindSceneViolations();

        if (violations.Count == 0)
            return;

        EditorApplication.isPlaying = false;
        EditorApplication.isPaused = false;

        ReferenceValidator.LogViolations(violations);
        Debug.LogWarning($"Запуск отменён: проблемных ссылок — {violations.Count}. Исправь их в Reference Wizard.");

        EditorApplication.delayCall -= ShowCancelledPlayMode;
        EditorApplication.delayCall += ShowCancelledPlayMode;
    }

    private static void ShowCancelledPlayMode()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        EditorApplication.isPaused = false;
        ReferenceWizard.Open();
    }

    [MenuItem("Tools/51 Percent/Restore Edit Mode")]
    private static void RestoreEditMode()
    {
        EditorApplication.isPlaying = false;
        EditorApplication.isPaused = false;
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }
}
