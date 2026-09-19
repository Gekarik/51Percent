using UnityEditor;
using UnityEngine;

/// Переводит импортёр модели в humanoid-риг.
/// Без аватара мышечные кривые не на что раскладывать, и ретаргет невозможен.
public sealed class HumanoidRigConfigurer
{
    public void Configure(string modelPath)
    {
        var importer = (ModelImporter)AssetImporter.GetAtPath(modelPath);

        importer.animationType = ModelImporterAnimationType.Human;
        importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        importer.importAnimation = true;
        importer.SaveAndReimport();
    }

    public bool HasValidAvatar(string modelPath)
    {
        foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(modelPath))
            if (asset is Avatar avatar)
                return avatar.isHuman && avatar.isValid;

        return false;
    }
}
