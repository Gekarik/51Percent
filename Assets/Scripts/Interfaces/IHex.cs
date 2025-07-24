using System;

public interface IHex
{
    event Action<ICharacter> StateChanged;
    HexState State { get;}
}