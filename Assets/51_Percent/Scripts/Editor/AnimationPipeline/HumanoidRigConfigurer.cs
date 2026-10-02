using UnityEditor;
using UnityEngine;

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
