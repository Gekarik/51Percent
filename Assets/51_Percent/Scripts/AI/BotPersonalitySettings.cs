using UnityEngine;

[CreateAssetMenu(fileName = "BotPersonality", menuName = "51_Percent/AI/Bot Personality")]
public class BotPersonalitySettings : ScriptableObject
{
    [Header("Поведение")]
    [Range(0f, 1f)]
    [Tooltip("Склонность атаковать других персонажей")]
    [SerializeField] private float _aggression = 0.5f;

    [Range(0f, 1f)]
    [Tooltip("Приоритет сбора коллектиблов; масштабирует радиус поиска")]
    [SerializeField] private float _greed = 0.5f;

    [Header("Имитация человека")]
    [Range(0.2f, 1f)]
    [Tooltip("Время между переоценкой целей (секунды)")]
    [SerializeField] private float _reactionTime = 0.4f;

    [Header("Технические")]
    [Range(1f, 15f)]
    [Tooltip("Радиус обнаружения целей")]
    [SerializeField] private float _detectionRadius = 8f;

    [Range(3, 25)]
    [Tooltip("Максимальная длина trail до принудительного возврата")]
    [SerializeField] private int _maxTrailLength = 10;

    public float Aggression => _aggression;
    public float Greed => _greed;
    public float ReactionTime => _reactionTime;
    public float DetectionRadius => _detectionRadius;
    public int MaxTrailLength => _maxTrailLength;
}
