using UnityEngine;

// Форма спуска модели при появлении персонажа, общая для всех префабов.
// Длительность сюда не входит: её задаёт доменное окно приземления
[CreateAssetMenu(fileName = "SpawnDescentSettings", menuName = "51_Percent/Spawn Descent Settings")]
public class SpawnDescentSettings : ScriptableObject
{
    private const float DefaultStartHeight = 3.5f;
    private const float DefaultGroundContact = 0.64f;
    private const float MinGroundContact = 0.05f;

    [SerializeField, Min(0f)] private float _startHeight = DefaultStartHeight;

    // Доля окна, на которой модель касается земли. Должна совпадать с кадром удара
    // в клипе приземления: остаток окна клип отыгрывает подъём из приседа
    [SerializeField, Range(MinGroundContact, 1f)] private float _groundContact = DefaultGroundContact;

    [SerializeField] private AnimationCurve _remainingHeight = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    // Высота над точкой появления в момент progress; после касания модель на земле
    public float GetHeight(float progress)
    {
        float descent = Mathf.Clamp01(progress / _groundContact);
        return _startHeight * _remainingHeight.Evaluate(descent);
    }
}
