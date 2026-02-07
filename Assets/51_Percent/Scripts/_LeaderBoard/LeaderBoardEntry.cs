public class LeaderBoardEntry
{
    public ICharacter Character { get; }
    public float Percent { get; private set; }

    public LeaderBoardEntry(ICharacter character, float percent = 0f)
    {
        Character = character;
        Percent = percent;
    }

    public void UpdatePercent(float percent)
    {
        Percent = percent;
    }
}
