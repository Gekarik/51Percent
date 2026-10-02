public sealed class AnimationImportRequest
{
    public AnimationImportRequest(string sourcePath, string clipName, bool loop)
    {
        SourcePath = sourcePath;
        ClipName = clipName;
        Loop = loop;
    }

    public string SourcePath { get; }
    public string ClipName { get; }
    public bool Loop { get; }
}
