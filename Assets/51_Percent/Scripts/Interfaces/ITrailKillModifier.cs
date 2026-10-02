// Эффект владельца трейла, меняющий исход атаки по нему (шипы разворачивают убийство).
// Боевые правила спрашивают об этом активный эффект напрямую, вместо того чтобы эффект
// регистрировал обработчик в менеджере убийств через персонажа.
public interface ITrailKillModifier
{
    (ICharacter victim, ICharacter killer) ResolveTrailKill(ICharacter trailOwner, ICharacter stepper);
}
