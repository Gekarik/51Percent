using System;
using System.IO;
using UnityEditor;
using UnityEngine;

/// Проводит анимацию от исходного файла до готового humanoid-клипа в проекте.
public sealed class AnimationImportPipeline
{
    // Суффикс "~" заставляет Unity полностью игнорировать папку: исходные GLB
    // лежат рядом с проектом, но не становятся ассетами и не плодят дубли клипов
    private const string GlbFolder = "Assets/Mesh2Motion/Glb~";
    private const string FbxFolder = "Assets/Mesh2Motion/Fbx";
    private const string ClipsFolder = "Assets/51_Percent/Animations";

    private const string GlbExtension = ".glb";
    private const string FbxExtension = ".fbx";
    private const string ClipExtension = ".anim";

    // Хвост короче этого порога — погрешность запекания, обрезать нечего
    private const float TailToleranceFrames = 2f;

    private readonly BlenderLocator _blenderLocator;
    private readonly HumanoidRigConfigurer _rigConfigurer;
    private readonly ClipImportConfigurer _clipConfigurer;
    private readonly StaticTailResolver _tailResolver;
    private readonly AnimationClipExtractor _extractor;

    public AnimationImportPipeline(BlenderLocator blenderLocator, HumanoidRigConfigurer rigConfigurer,
        ClipImportConfigurer clipConfigurer, StaticTailResolver tailResolver, AnimationClipExtractor extractor)
    {
        _blenderLocator = blenderLocator;
        _rigConfigurer = rigConfigurer;
        _clipConfigurer = clipConfigurer;
        _tailResolver = tailResolver;
        _extractor = extractor;
    }

    public AnimationImportReport Run(AnimationImportRequest request)
    {
        var report = new AnimationImportReport();

        if (!File.Exists(request.SourcePath))
        {
            report.Fail("Файл не найден: " + request.SourcePath);
            return report;
        }

        if (string.IsNullOrWhiteSpace(request.ClipName))
        {
            report.Fail("Не задано имя клипа.");
            return report;
        }

        string modelPath = FbxFolder + "/" + request.ClipName + FbxExtension;

        if (!TryPlaceModel(request, modelPath, report))
            return report;

        ConfigureRig(modelPath, report);

        if (!_rigConfigurer.HasValidAvatar(modelPath))
        {
            report.Fail("Unity не собрал humanoid-аватар. Проверь именование костей в исходнике.");
            return report;
        }

        ApplyClipSettings(request, modelPath, report);

        if (!ExtractClip(request, modelPath, report))
            return report;

        RetireModelClip(modelPath, report);
        return report;
    }

    private bool TryPlaceModel(AnimationImportRequest request, string modelPath, AnimationImportReport report)
    {
        EnsureFolder(FbxFolder);

        if (IsGlb(request.SourcePath))
        {
            if (!TryConvert(request.SourcePath, modelPath, report))
                return false;

            ArchiveSource(request, report);
        }
        else
        {
            File.Copy(request.SourcePath, modelPath, true);
            report.AddStep("FBX скопирован в " + FbxFolder);
        }

        AssetDatabase.ImportAsset(modelPath, ImportAssetOptions.ForceUpdate);
        return true;
    }

    private bool IsGlb(string path)
    {
        return Path.GetExtension(path).Equals(GlbExtension, StringComparison.OrdinalIgnoreCase);
    }

    private void ArchiveSource(AnimationImportRequest request, AnimationImportReport report)
    {
        EnsureFolder(GlbFolder);
        File.Copy(request.SourcePath, GlbFolder + "/" + request.ClipName + GlbExtension, true);
        report.AddStep("Исходный GLB сохранён в " + GlbFolder);
    }

    private bool TryConvert(string sourcePath, string modelPath, AnimationImportReport report)
    {
        string blenderPath = _blenderLocator.Resolve();

        if (string.IsNullOrEmpty(blenderPath))
        {
            report.Fail("Blender не найден. Укажи путь к blender.exe в окне конвейера.");
            return false;
        }

        var converter = new GlbToFbxConverter(blenderPath);

        if (!converter.TryConvert(sourcePath, Path.GetFullPath(modelPath), out string log))
        {
            report.Fail("Blender не создал FBX. " + log);
            return false;
        }

        report.AddStep("GLB сконвертирован в FBX. " + log);
        return true;
    }

    private void ConfigureRig(string modelPath, AnimationImportReport report)
    {
        _rigConfigurer.Configure(modelPath);
        report.AddStep("Риг переведён в Humanoid.");
    }

    private void ApplyClipSettings(AnimationImportRequest request, string modelPath, AnimationImportReport report)
    {
        AnimationClip imported = _extractor.FindClip(modelPath);
        int lastFrame = ResolveLastFrame(imported, report);

        _clipConfigurer.Apply(modelPath, request.Loop, lastFrame);
        report.AddStep(request.Loop ? "Зацикливание включено." : "Зацикливание выключено.");
    }

    private int ResolveLastFrame(AnimationClip clip, AnimationImportReport report)
    {
        if (clip == null)
            return 0;

        float lastTime = _tailResolver.ResolveLastMeaningfulTime(clip);

        if (lastTime <= 0f)
            return 0;

        float meaningfulFrames = lastTime * clip.frameRate;
        float totalFrames = clip.length * clip.frameRate;

        if (totalFrames - meaningfulFrames <= TailToleranceFrames)
            return 0;

        int lastFrame = Mathf.CeilToInt(meaningfulFrames);
        report.AddStep($"Обрезан статичный хвост: {totalFrames:F0} -> {lastFrame} кадров.");
        return lastFrame;
    }

    private bool ExtractClip(AnimationImportRequest request, string modelPath, AnimationImportReport report)
    {
        EnsureFolder(ClipsFolder);
        string clipPath = ClipsFolder + "/" + request.ClipName + ClipExtension;

        AnimationClip extracted = _extractor.Extract(modelPath, clipPath, request.ClipName);

        if (extracted == null)
        {
            report.Fail("В модели нет анимационного клипа.");
            return false;
        }

        report.AddStep($"Клип извлечён: {extracted.length:F3} с, humanMotion={extracted.humanMotion}.");
        report.Complete(clipPath);
        return true;
    }

    private void RetireModelClip(string modelPath, AnimationImportReport report)
    {
        _clipConfigurer.DisableImport(modelPath);
        report.AddStep("Клип внутри FBX отключён — в проекте остался один источник.");
    }

    private void EnsureFolder(string folder)
    {
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
    }
}
