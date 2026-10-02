using UnityEngine;

public interface IBoosterContext
{
    bool CanAct { get; }
    CharacterStats Stats { get; }
    void EscapeToTerritory();
    void SetTrailMesh(Mesh mesh);
    void ClearTrailMesh();
}
