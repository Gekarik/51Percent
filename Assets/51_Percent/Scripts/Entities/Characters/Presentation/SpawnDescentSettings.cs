using UnityEngine;

[CreateAssetMenu(fileName = "SpawnDescentSettings", menuName = "51_Percent/Spawn Descent Settings")]
public class SpawnDescentSettings : ScriptableObject
{
    private const float DefaultStartHeight = 3.5f;
    private const float DefaultGroundContact = 0.64f;
    private const float MinGroundContact = 0.05f;

    [SerializeField, Min(0f)] private float _startHeight = DefaultStartHeight;

    [SerializeField, Range(MinGroundContact, 1f)] private float _groundContact = DefaultGroundContact;

    [SerializeField] private AnimationCurve _remainingHeight = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    public float GetHeight(float progress)
    {
        float descent = Mathf.Clamp01(progress / _groundContact);
        return _startHeight * _remainingHeight.Evaluate(descent);
    }
}
