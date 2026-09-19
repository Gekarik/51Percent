// Источник режима походки для бустера. Узкий интерфейс: переключателю походки
// не нужны ни пропы, ни масштаб
public interface ILocomotionModeSource
{
    LocomotionMode GetMode(BoosterId id);
}
