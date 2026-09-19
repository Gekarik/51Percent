// Источник описания пропа. TryGet, а не молчаливый дефолт: незаполненная запись
// должна быть заметной ошибкой настройки, а не невидимым бездействием
public interface IBoosterPropSource
{
    bool TryGetProp(BoosterId id, out BoosterPropPresentation prop);
}
