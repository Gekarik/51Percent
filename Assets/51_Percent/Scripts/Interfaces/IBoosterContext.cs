using System;
using UnityEngine;

public interface IBoosterContext
{
    CharacterStats Stats { get; }
    event Action Died;
    void RequestRespawn();
    void RegisterTrailKillResolver(Func<ICharacter, ICharacter, (ICharacter victim, ICharacter killer)> resolver);
    void UnregisterTrailKillResolver();
    void SetModelScale(float factor);
    void SetTrailMesh(Mesh mesh);
    void ClearTrailMesh();
}
