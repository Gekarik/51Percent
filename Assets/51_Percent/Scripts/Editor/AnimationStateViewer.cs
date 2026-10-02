using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class AnimationStateViewer : EditorWindow
{
    private const int Layer = 0;
    private const float BarHeight = 18f;

    private readonly Dictionary<int, string> _stateNames = new Dictionary<int, string>();
    private readonly AnimationPreviewer _previewer = new AnimationPreviewer();
    private readonly AnimationStatePlayer _statePlayer = new AnimationStatePlayer();

    private RuntimeAnimatorController _cachedController;
    private AnimationClip _previewClip;
    private Vector2 _scroll;

    [MenuItem("Tools/51 Percent/Animation State Viewer")]
    private static void Open() => GetWindow<AnimationStateViewer>("Animation State");

    private void OnEnable() => EditorApplication.update += OnEditorUpdate;

    private void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
        _previewer.Stop();
    }

    private void OnEditorUpdate()
    {
        _previewer.Tick();
        Repaint();
    }

    private void OnGUI()
    {
        var animator = ResolveAnimator();

        if (animator == null)
        {
            EditorGUILayout.HelpBox("Выдели объект с Animator. Подойдёт и корень персонажа, и открытый префаб в Prefab Stage.", MessageType.Info);
            return;
        }

        EditorGUILayout.LabelField("Объект", animator.gameObject.name, EditorStyles.boldLabel);
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        if (EditorApplication.isPlaying)
            DrawPlayMode(animator);
        else
            DrawEditMode(animator);

        EditorGUILayout.EndScrollView();
    }

    private static Animator ResolveAnimator()
    {
        var selected = Selection.activeGameObject;
        return selected == null ? null : selected.GetComponentInChildren<Animator>();
    }

    private void DrawPlayMode(Animator animator)
    {
        CacheStateNames(animator);
        DrawCurrentState(animator);
        DrawTransition(animator);
        DrawParameters(animator);
        DrawCharacter(animator);
        DrawStateButtons(animator);
    }

    private void DrawCurrentState(Animator animator)
    {
        var info = animator.GetCurrentAnimatorStateInfo(Layer);
        float progress = info.normalizedTime % 1f;

        EditorGUILayout.LabelField("Состояние", ResolveState(info.shortNameHash));
        EditorGUILayout.LabelField("Клип", DescribeClips(animator));
        EditorGUILayout.LabelField("Длина", $"{info.length:F3} с,  скорость {info.speed:F2}");
        Bar(progress, $"проиграно {progress * 100f:F0}%");
    }

    private static string DescribeClips(Animator animator)
    {
        var clips = animator.GetCurrentAnimatorClipInfo(Layer);

        if (clips.Length == 0)
            return "клип не назначен";

        var names = new List<string>();

        foreach (var info in clips)
            names.Add(info.clip == null ? "пусто" : $"{info.clip.name} ({info.weight:F2})");

        return string.Join(",  ", names);
    }

    private void DrawTransition(Animator animator)
    {
        if (!animator.IsInTransition(Layer))
            return;

        var next = animator.GetNextAnimatorStateInfo(Layer);
        var transition = animator.GetAnimatorTransitionInfo(Layer);
        Bar(transition.normalizedTime, $"переход в {ResolveState(next.shortNameHash)}");
    }

    private void DrawParameters(Animator animator)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Параметры", EditorStyles.boldLabel);

        foreach (var parameter in animator.parameters)
            EditorGUILayout.LabelField(parameter.name, ReadParameter(animator, parameter));
    }

    private static string ReadParameter(Animator animator, AnimatorControllerParameter parameter)
    {
        switch (parameter.type)
        {
            case AnimatorControllerParameterType.Bool: return animator.GetBool(parameter.name).ToString();
            case AnimatorControllerParameterType.Int: return animator.GetInteger(parameter.name).ToString();
            case AnimatorControllerParameterType.Float: return animator.GetFloat(parameter.name).ToString("F3");
            default: return "trigger";
        }
    }

    private void DrawCharacter(Animator animator)
    {
        var character = animator.GetComponentInParent<CharacterBase>();

        if (character == null)
            return;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Персонаж", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Окно приземления", character.IsLanding ? "открыто" : "закрыто");
        EditorGUILayout.LabelField("Высота модели", $"{animator.transform.localPosition.y:F3} (локальная)");
        EditorGUILayout.LabelField("Скорость", $"{character.Speed:F3}");
    }

    private void DrawStateButtons(Animator animator)
    {
        var names = _statePlayer.GetStateNames(animator);

        if (names.Count == 0)
            return;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Запустить состояние", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Запуск в обход условий графа. Игровая логика может переключить состояние обратно уже на следующем кадре.", MessageType.None);

        foreach (string name in names)
            if (GUILayout.Button(name))
                _statePlayer.Play(animator, name);
    }

    private void DrawEditMode(Animator animator)
    {
        EditorGUILayout.LabelField("Просмотр клипа без запуска игры", EditorStyles.boldLabel);
        _previewClip = (AnimationClip)EditorGUILayout.ObjectField("Клип", _previewClip, typeof(AnimationClip), false);

        if (_previewClip == null)
        {
            EditorGUILayout.HelpBox("Выбери клип, чтобы проиграть его на этом объекте.", MessageType.Info);
            return;
        }

        var target = animator.gameObject;

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button(_previewer.IsPlaying ? "Пауза" : "Играть"))
            TogglePreview(target);

        if (GUILayout.Button("Стоп"))
            _previewer.Stop();

        EditorGUILayout.EndHorizontal();

        float time = EditorGUILayout.Slider("Время", _previewer.Time, 0f, _previewClip.length);

        if (!Mathf.Approximately(time, _previewer.Time))
            _previewer.Scrub(target, _previewClip, time);

        EditorGUILayout.LabelField("Длина клипа", $"{_previewClip.length:F3} с");
    }

    private void TogglePreview(GameObject target)
    {
        if (_previewer.IsPlaying)
            _previewer.Pause();
        else
            _previewer.Play(target, _previewClip);
    }

    private void CacheStateNames(Animator animator)
    {
        if (_cachedController == animator.runtimeAnimatorController && _stateNames.Count > 0)
            return;

        _cachedController = animator.runtimeAnimatorController;
        _stateNames.Clear();

        if (_cachedController is AnimatorController controller)
            foreach (var layer in controller.layers)
                foreach (var child in layer.stateMachine.states)
                    _stateNames[Animator.StringToHash(child.state.name)] = child.state.name;
    }

    private string ResolveState(int hash)
    {
        return _stateNames.TryGetValue(hash, out string name) ? name : "имя не найдено в графе";
    }

    private static void Bar(float value, string label)
    {
        var rect = EditorGUILayout.GetControlRect(false, BarHeight);
        EditorGUI.ProgressBar(rect, Mathf.Clamp01(value), label);
    }
}
