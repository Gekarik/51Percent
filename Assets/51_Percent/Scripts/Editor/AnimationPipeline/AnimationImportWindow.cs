using System.IO;
using UnityEditor;
using UnityEngine;

public sealed class AnimationImportWindow : EditorWindow
{
    private const string WindowTitle = "Импорт анимаций";
    private const string SourceFilter = "glb,fbx";
    private const float LabelWidth = 110f;

    private readonly BlenderLocator _blenderLocator = new BlenderLocator();

    private string _sourcePath = string.Empty;
    private string _clipName = string.Empty;
    private string _blenderPath = string.Empty;
    private bool _loop = true;
    private AnimationImportReport _report;
    private Vector2 _scroll;

    [MenuItem("Tools/51 Percent/Импорт анимаций")]
    private static void Open()
    {
        GetWindow<AnimationImportWindow>(WindowTitle);
    }

    private void OnEnable()
    {
        _blenderPath = _blenderLocator.Resolve();
    }

    private void OnGUI()
    {
        EditorGUIUtility.labelWidth = LabelWidth;

        DrawSourceField();
        _clipName = EditorGUILayout.TextField("Имя клипа", _clipName);
        _loop = EditorGUILayout.Toggle("Зациклить", _loop);

        DrawBlenderField();
        EditorGUILayout.Space();
        DrawImportButton();
        DrawReport();
    }

    private void DrawSourceField()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            _sourcePath = EditorGUILayout.TextField("Исходник", _sourcePath);

            if (GUILayout.Button("...", GUILayout.Width(30f)))
                SelectSource();
        }
    }

    private void SelectSource()
    {
        string path = EditorUtility.OpenFilePanel("Выбери GLB или FBX", string.Empty, SourceFilter);

        if (string.IsNullOrEmpty(path))
            return;

        _sourcePath = path;

        if (string.IsNullOrWhiteSpace(_clipName))
            _clipName = Path.GetFileNameWithoutExtension(path);
    }

    private void DrawBlenderField()
    {
        if (!IsGlbSelected())
            return;

        using (new EditorGUILayout.HorizontalScope())
        {
            _blenderPath = EditorGUILayout.TextField("Blender", _blenderPath);

            if (GUILayout.Button("...", GUILayout.Width(30f)))
                SelectBlender();
        }

        if (string.IsNullOrEmpty(_blenderPath))
            EditorGUILayout.HelpBox("Blender не найден — укажи blender.exe вручную.", MessageType.Warning);
    }

    private void SelectBlender()
    {
        string path = EditorUtility.OpenFilePanel("Укажи blender.exe", string.Empty, "exe");

        if (string.IsNullOrEmpty(path))
            return;

        _blenderPath = path;
        _blenderLocator.Remember(path);
    }

    private bool IsGlbSelected()
    {
        return _sourcePath.EndsWith(".glb", System.StringComparison.OrdinalIgnoreCase);
    }

    private void DrawImportButton()
    {
        using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(_sourcePath)))
        {
            if (GUILayout.Button("Импортировать"))
                Import();
        }
    }

    private void Import()
    {
        if (!string.IsNullOrEmpty(_blenderPath))
            _blenderLocator.Remember(_blenderPath);

        var pipeline = new AnimationImportPipeline(
            _blenderLocator,
            new HumanoidRigConfigurer(),
            new ClipImportConfigurer(),
            new StaticTailResolver(),
            new AnimationClipExtractor());

        _report = pipeline.Run(new AnimationImportRequest(_sourcePath, _clipName, _loop));

        if (_report.Succeeded)
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<AnimationClip>(_report.ClipPath);
    }

    private void DrawReport()
    {
        if (_report == null)
            return;

        EditorGUILayout.Space();

        using (var scope = new EditorGUILayout.ScrollViewScope(_scroll))
        {
            _scroll = scope.scrollPosition;

            foreach (string step in _report.Steps)
                EditorGUILayout.LabelField("• " + step, EditorStyles.wordWrappedLabel);

            if (_report.Succeeded)
                EditorGUILayout.HelpBox("Готово: " + _report.ClipPath, MessageType.Info);
            else if (_report.Error != null)
                EditorGUILayout.HelpBox(_report.Error, MessageType.Error);
        }
    }
}
