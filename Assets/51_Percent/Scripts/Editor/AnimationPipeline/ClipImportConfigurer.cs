using UnityEditor;

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

    public void DisableImport(string modelPath)
    {
        var importer = (ModelImporter)AssetImporter.GetAtPath(modelPath);

        importer.importAnimation = false;
        importer.SaveAndReimport();
    }

    private ModelImporterClipAnimation[] ResolveClips(ModelImporter importer)
    {
        return importer.clipAnimations.Length > 0
            ? importer.clipAnimations
            : importer.defaultClipAnimations;
    }
}
