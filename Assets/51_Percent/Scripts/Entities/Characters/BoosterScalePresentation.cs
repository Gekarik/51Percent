using DG.Tweening;

// Срез реестра для роста модели
public readonly struct BoosterScalePresentation
{
    public float Factor { get; }
    public float GrowDuration { get; }
    public float ShrinkDuration { get; }
    public Ease ScaleEase { get; }

    public BoosterScalePresentation(float factor, float growDuration, float shrinkDuration, Ease ease)
    {
        Factor = factor;
        GrowDuration = growDuration;
        ShrinkDuration = shrinkDuration;
        ScaleEase = ease;
    }
}
