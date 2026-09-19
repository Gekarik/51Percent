// Источник описания роста модели
public interface IBoosterScaleSource
{
    bool TryGetScale(BoosterId id, out BoosterScalePresentation scale);
}
