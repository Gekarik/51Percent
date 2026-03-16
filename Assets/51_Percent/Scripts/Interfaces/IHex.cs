using System;
using UnityEngine;

public interface IHex
{
    Transform Transform { get; }
    HexState State { get; }
    ICharacter Owner { get; }
    Transform ViewTransform { get; }

    event Action<IHex> StateChanged;

    void SetOwner(ICharacter owner, HexState state);
    void Reset();
}
