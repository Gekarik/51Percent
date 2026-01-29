using UnityEngine;

[CreateAssetMenu(fileName = "BotPersonality", menuName = "51_Percent/Bot Personality")]
public class BotPersonalitySettings : ScriptableObject
{
    [Header("Поведение")]
    [Range(0f, 1f)]
    [Tooltip("Склонность атаковать других персонажей")]
    [SerializeField] private float _aggression = 0.5f;

    [Range(0f, 1f)]
    [Tooltip("Готовность рисковать (создавать длинные trail)")]
    [SerializeField] private float _riskTolerance = 0.5f;

    [Range(0f, 1f)]
    [Tooltip("Приоритет сбора монеток")]
    [SerializeField] private float _greed = 0.5f;

    [Range(0f, 1f)]
    [Tooltip("Приоритет захвата территории")]
    [SerializeField] private float _territorialFocus = 0.5f;

    [Header("Имитация человека")]
    [Range(0.2f, 1f)]
    [Tooltip("Время между переоценкой целей (секунды)")]
    [SerializeField] private float _reactionTime = 0.4f;

    [Range(0f, 0.5f)]
    [Tooltip("Случайность в принятии решений")]
    [SerializeField] private float _randomness = 0.2f;

    [Header("Технические")]
    [Range(1f, 15f)]
    [Tooltip("Радиус обнаружения целей")]
    [SerializeField] private float _detectionRadius = 8f;

    [Range(3, 25)]
    [Tooltip("Максимальная длина trail до принудительного возврата")]
    [SerializeField] private int _maxTrailLength = 10;

    public float Aggression => _aggression;
    public float RiskTolerance => _riskTolerance;
    public float Greed => _greed;
    public float TerritorialFocus => _territorialFocus;
    public float ReactionTime => _reactionTime;
    public float Randomness => _randomness;
    public float DetectionRadius => _detectionRadius;
    public int MaxTrailLength => _maxTrailLength;
}
