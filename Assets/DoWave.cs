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
    [Tooltip("�������, �� ������� ����� ������������� �����")]
    [SerializeField] private Transform[] _targets;

    [Header("Wave Settings")]
    [Tooltip("���� ������ (��������� �� Y)")]
    [SerializeField] private float jumpPower = 2f;
    [Tooltip("������������ ������ ������ (� ��������)")]
    [SerializeField] private float duration = 0.5f;
    [Tooltip("�������� ������� �������� ������� (� ��������)")]
    [SerializeField] private float delayBetween = 0.1f;
    [Tooltip("���������� �������� ���� ����� (1 = ���� ������)")]
    [SerializeField] private int waveRepeats = 1;
    [Tooltip("��� ��������� (Easing) � ����� ������ �� Linear, InOutSine � �.�.")]
    [SerializeField] private Ease easeType = Ease.Linear;

    [Header("Use Local Coordinates?")]
    [Tooltip("���� �������� � ����� ������� �� ��������� �����������")]
    [SerializeField] private bool useLocal = false;

    // ���������� ����
    private float[] _initialYs;
    private Sequence waveSequence;

    // ��� ������������ ������
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

        // ������� dt �� ������ ������������ �������
        double now = EditorApplication.timeSinceStartup;
        float delta = (float)(now - lastEditorTime);
        lastEditorTime = now;

        previewTime += delta;

        // ����������� sequence
        waveSequence.Goto(previewTime, andPlay: false);

        // �����, ����� ������ ��-�����������
        foreach (var t in _targets)
            if (t != null)
                EditorUtility.SetDirty(t);
    }
#endif

    #endregion

    #region Wave Building & Control

    private void Start()
    {
        // � PlayMode ����� ������ ���������
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
        // ������� �������������, ������ ������
        InitializeTargetsAndYs();
        BuildWave();

#if UNITY_EDITOR
        // ��������� ������������ ������
        isPreviewing = true;
        lastEditorTime = EditorApplication.timeSinceStartup;
        previewTime = 0;
#else
        // � PlayMode � ������ ����������� Tween
        waveSequence.Restart();
#endif
    }

    private void OnDestroy()
    {
        waveSequence?.Kill();
    }

    #endregion
}
