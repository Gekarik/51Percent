using System;

// Что нужно персонажу, чтобы появиться в матче — дословно аргументы CharacterBase.Init.
// Это сгруппированные параметры, а не контейнер: состав фиксирован в объявлении, поиска
// по типу нет, каждое поле названо. Имя намеренно узкое — пока тип называется
// «зависимости персонажа», всё, что персонажу не нужно, здесь лежать не может.
// На весь матч эти службы одни и те же, поэтому объект создаётся один раз в Bootstrap
public class CharacterDependencies
{
    public CharacterDependencies(ColorService colorService, TerritoryManager territory,
        IHexGridProvider grid, IMatchState matchState)
    {
        ColorService = colorService ?? throw new ArgumentNullException(nameof(colorService));
        Territory = territory != null ? territory : throw new ArgumentNullException(nameof(territory));
        Grid = grid ?? throw new ArgumentNullException(nameof(grid));
        MatchState = matchState ?? throw new ArgumentNullException(nameof(matchState));
    }

    public ColorService ColorService { get; }
    public TerritoryManager Territory { get; }
    public IHexGridProvider Grid { get; }
    public IMatchState MatchState { get; }
}
