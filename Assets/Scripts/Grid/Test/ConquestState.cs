using System.Collections.Generic;

public class ConquestState : IConquestState
{
    public ISet<Hex> FixedHexes { get; }
    public IList<Hex> TrailHexes { get; }

    public ConquestState(ISet<Hex> fixedHexes, IList<Hex> trailHexes)
    {
        FixedHexes = fixedHexes;
        TrailHexes = trailHexes;
    }
}