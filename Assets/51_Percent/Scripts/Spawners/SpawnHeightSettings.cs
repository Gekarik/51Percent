using UnityEngine;

[CreateAssetMenu(fileName = "SpawnHeightSettings", menuName = "51_Percent/Spawn Height Settings")]
public class SpawnHeightSettings : ScriptableObject
{
    // Абсолютная мировая высота, на которой висят все коллектиблы над плоской картой
    [SerializeField] private float _worldHeight = 1f;

    public float WorldHeight => _worldHeight;
}
