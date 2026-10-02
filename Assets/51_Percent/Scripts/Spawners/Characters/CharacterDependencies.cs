using System;

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
