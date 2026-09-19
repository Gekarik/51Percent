using UnityEditor;

/// Настраивает, как модель отдаёт свой клип: границы, зацикливание, наличие клипа вообще.
public sealed class ClipImportConfigurer
{
    public void Apply(string modelPath, bool loop, int lastFrame)
    {
        var importer = (ModelImporter)AssetImporter.GetAtPath(modelPath);
        ModelImporterClipAnimation[] clips = ResolveClips(importer);

        if (clips.Length == 0)
            return;

        clips[0].loopTime = loop;

        if (lastFrame > clips[0].firstFrame)
            clips[0].lastFrame = lastFrame;

        importer.clipAnimations = clips;
        importer.SaveAndReimport();
    }

    /// После извлечения клип внутри модели становится дублем отдельного ассета:
    /// в списках выбора одно и то же движение предлагалось бы дважды.
    /// Аватар при этом сохраняется — он собирается из рига, а не из анимации.
    public void DisableImport(string modelPath)
    {
        var importer = (ModelImporter)AssetImporter.GetAtPath(modelPath);

        importer.importAnimation = false;
        importer.SaveAndReimport();
    }

    // До первой правки clipAnimations пуст, а настройки лежат в defaultClipAnimations
    private ModelImporterClipAnimation[] ResolveClips(ModelImporter importer)
    {
        return importer.clipAnimations.Length > 0
            ? importer.clipAnimations
            : importer.defaultClipAnimations;
    }
}
