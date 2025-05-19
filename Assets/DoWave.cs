#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Linq;
using UnityEngine;
using DG.Tweening;

[ExecuteInEditMode]
public class WaveJump : MonoBehaviour
{
    [Header("Targets")]
    [Tooltip("Объекты, по которым будет прокатываться волна")]
    [SerializeField] private Transform[] _targets;

    [Header("Wave Settings")]
    [Tooltip("Сила прыжка (амплитуда по Y)")]
    [SerializeField] private float jumpPower = 2f;
    [Tooltip("Длительность одного прыжка (в секундах)")]
    [SerializeField] private float duration = 0.5f;
    [Tooltip("Задержка запуска соседних прыжков (в секундах)")]
    [SerializeField] private float delayBetween = 0.1f;
    [Tooltip("Количество повторов всей волны (1 = один прогон)")]
    [SerializeField] private int waveRepeats = 1;
    [Tooltip("Тип плавности (Easing) — можно менять на Linear, InOutSine и т.д.")]
    [SerializeField] private Ease easeType = Ease.Linear;

    [Header("Use Local Coordinates?")]
    [Tooltip("Если включено — будет прыгать по локальным координатам")]
    [SerializeField] private bool useLocal = false;

    // Внутренние поля
    private float[] _initialYs;
    private Sequence waveSequence;

    // Для редакторного превью
    private bool isPreviewing = false;
    private double lastEditorTime = 0;
    private float previewTime = 0;

    #region Initialization

    private void OnValidate() => InitializeTargetsAndYs();

    private void Awake() => InitializeTargetsAndYs();

    private void InitializeTargetsAndYs()
    {
        if (_targets == null || _targets.Length == 0)
            _targets = GetComponentsInChildren<Transform>()
                       .Where(t => t != transform)
                       .ToArray();

        if (_initialYs == null || _initialYs.Length != _targets.Length)
            _initialYs = new float[_targets.Length];

        for (int i = 0; i < _targets.Length; i++)
        {
            if (_targets[i] == null) continue;
            _initialYs[i] = useLocal
                ? _targets[i].localPosition.y
                : _targets[i].position.y;
        }
    }

    #endregion

    #region Editor Update Loop

#if UNITY_EDITOR
    private void OnEnable()
    {
        EditorApplication.update += EditorUpdate;
    }

    private void OnDisable()
    {
        EditorApplication.update -= EditorUpdate;
    }

    private void EditorUpdate()
    {
        if (!isPreviewing || waveSequence == null) return;

        // считаем dt на основе редакторного времени
        double now = EditorApplication.timeSinceStartup;
        float delta = (float)(now - lastEditorTime);
        lastEditorTime = now;

        previewTime += delta;

        // Прокатываем sequence
        waveSequence.Goto(previewTime, andPlay: false);

        // Нужно, чтобы сценка ре-рендерилась
        foreach (var t in _targets)
            if (t != null)
                EditorUtility.SetDirty(t);
    }
#endif

    #endregion

    #region Wave Building & Control

    private void Start()
    {
        // В PlayMode можно просто запустить
        BuildWave();
        waveSequence.Play();
    }

    private void BuildWave()
    {
        InitializeTargetsAndYs();

        waveSequence?.Kill();
        waveSequence = DOTween.Sequence();

        for (int i = 0; i < _targets.Length; i++)
        {
            var t = _targets[i];
            if (t == null) continue;

            float startY = _initialYs[i];
            float peakY = startY + jumpPower;

            Tween moveTween = useLocal
                ? t.DOLocalMoveY(peakY, duration * 0.5f)
                : t.DOMoveY(peakY, duration * 0.5f);

            moveTween
                .SetEase(easeType)
                .SetLoops(2, LoopType.Yoyo)
                .SetDelay(i * delayBetween);

            waveSequence.Insert(i * delayBetween, moveTween);
        }

        waveSequence.SetLoops(waveRepeats, LoopType.Restart);
    }

    [ContextMenu("Play Wave")]
    public void PlayWave()
    {
        // Сбросим инициализацию, соберём заново
        InitializeTargetsAndYs();
        BuildWave();

#if UNITY_EDITOR
        // Запускаем редакторский превью
        isPreviewing = true;
        lastEditorTime = EditorApplication.timeSinceStartup;
        previewTime = 0;
#else
        // В PlayMode — просто проигрываем Tween
        waveSequence.Restart();
#endif
    }

    private void OnDestroy()
    {
        waveSequence?.Kill();
    }

    #endregion
}
