using System;
using System.Collections.Generic;

public interface ITerritoryWriter : ITerritoryChanges
{
    IReadOnlyCollection<IHex> GetFixedByOwner(ICharacter character);
    bool IsFixedBy(ICharacter character, IHex hex);
    void FixHex(ICharacter character, IHex hex);
    void ReleaseHex(IHex hex);
    void TrailHex(ICharacter character, IHex hex);
    IReadOnlyList<IHex> CaptureEnclosedArea(ICharacter owner, IReadOnlyCollection<IHex> trail);
    void ReleaseDisconnectedFragments(ICharacter character);
    void OnCharacterDied(ICharacter character);
}
