using System;
using UnityEngine;

public interface IBoosterContext
{
    bool CanAct { get; }
    CharacterStats Stats { get; }
    void RegisterTrailKillResolver(Func<ICharacter, ICharacter, (ICharacter victim, ICharacter killer)> resolver);
    void UnregisterTrailKillResolver();
    void RegisterDeathInterceptor(Func<bool> interceptor);
    void UnregisterDeathInterceptor();
    void EscapeToTerritory();
    void SetTrailMesh(Mesh mesh);
    void ClearTrailMesh();
}
