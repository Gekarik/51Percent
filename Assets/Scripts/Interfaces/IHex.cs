using System;
using UnityEngine;

public interface IHex
{
    HexView HexView { get; }
    Transform transform { get; }
    ICharacter Owner { get; }
    event Action<IHex> StateChanged;
    HexState State { get; }
    void Reset();
    void SetOwner(ICharacter owner, HexState busy);
}
