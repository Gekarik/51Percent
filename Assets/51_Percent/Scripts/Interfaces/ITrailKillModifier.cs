public interface ITrailKillModifier
{
    (ICharacter victim, ICharacter killer) ResolveTrailKill(ICharacter trailOwner, ICharacter stepper);
}
