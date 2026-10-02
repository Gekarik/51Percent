using System;
using System.Collections.Generic;

// Операции над владением клетками, нужные правилам трейла. Интерфейс введён не ради
// самого интерфейса: он позволяет держать правила захвата обычным объектом и проверять их
// без сцены. Проверки согласованности по-прежнему выполняет владелец состояния — TerritoryManager.
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
