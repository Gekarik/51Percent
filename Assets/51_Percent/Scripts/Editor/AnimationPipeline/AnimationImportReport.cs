using System.Collections.Generic;

/// Накопленный результат прогона: пройденные шаги и итог.
public sealed class AnimationImportReport
{
    private readonly List<string> _steps = new List<string>();

    public string ClipPath { get; private set; }
    public string Error { get; private set; }
    public IReadOnlyList<string> Steps => _steps;
    public bool Succeeded => Error == null && ClipPath != null;

    public void AddStep(string step)
    {
        _steps.Add(step);
    }

    public void Complete(string clipPath)
    {
        ClipPath = clipPath;
    }

    public void Fail(string error)
    {
        Error = error;
    }
}
