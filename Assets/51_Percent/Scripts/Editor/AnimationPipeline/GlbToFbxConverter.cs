using System.IO;
using System.Text;
using Process = System.Diagnostics.Process;
using ProcessStartInfo = System.Diagnostics.ProcessStartInfo;

/// Конвертирует GLB в FBX внешним вызовом Blender в фоновом режиме.
public sealed class GlbToFbxConverter
{
    private const int TimeoutMilliseconds = 180000;
    private const string ScriptFileName = "glb_to_fbx.py";

    // Диапазон кадров сцены обязан совпадать с длиной экшена: по умолчанию у Blender
    // это 1-250, и экспортёр добивает клип статичной позой до конца диапазона.
    private const string ConversionScript = @"
import bpy, sys

argv = sys.argv[sys.argv.index(""--"") + 1:]
src, dst = argv[0], argv[1]

bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=src)

scene = bpy.context.scene
starts, ends = [], []
for action in bpy.data.actions:
    starts.append(action.frame_range[0])
    ends.append(action.frame_range[1])

if starts:
    scene.frame_start = int(round(min(starts)))
    scene.frame_end = int(round(max(ends)))

bpy.ops.export_scene.fbx(
    filepath=dst,
    use_selection=False,
    object_types={'ARMATURE', 'MESH'},
    add_leaf_bones=False,
    bake_anim=True,
    bake_anim_use_all_bones=True,
    bake_anim_use_nla_strips=False,
    bake_anim_use_all_actions=False,
    bake_anim_force_startend_keying=True,
    apply_scale_options='FBX_SCALE_ALL',
    axis_forward='-Z',
    axis_up='Y',
)
print('PIPELINE_RANGE:', scene.frame_start, scene.frame_end)
";

    private readonly string _blenderPath;

    public GlbToFbxConverter(string blenderPath)
    {
        _blenderPath = blenderPath;
    }

    public bool TryConvert(string glbPath, string fbxPath, out string log)
    {
        string scriptPath = WriteScript();

        using (var process = new Process())
        {
            process.StartInfo = BuildStartInfo(scriptPath, glbPath, fbxPath);
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string errors = process.StandardError.ReadToEnd();
            process.WaitForExit(TimeoutMilliseconds);

            log = Summarize(output, errors);
        }

        return File.Exists(fbxPath);
    }

    private ProcessStartInfo BuildStartInfo(string scriptPath, string glbPath, string fbxPath)
    {
        return new ProcessStartInfo(_blenderPath)
        {
            Arguments = $"-b --factory-startup -noaudio --python \"{scriptPath}\" -- \"{glbPath}\" \"{fbxPath}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };
    }

    private string WriteScript()
    {
        string path = Path.Combine(Path.GetTempPath(), ScriptFileName);
        File.WriteAllText(path, ConversionScript);
        return path;
    }

    // Blender пишет в stdout сотни строк, в отчёт нужны только диапазон и ошибки
    private string Summarize(string output, string errors)
    {
        var summary = new StringBuilder();

        foreach (string line in output.Split('\n'))
            if (line.StartsWith("PIPELINE_RANGE:"))
                summary.AppendLine(line.Trim());

        if (!string.IsNullOrWhiteSpace(errors))
            summary.AppendLine(errors.Trim());

        return summary.ToString().Trim();
    }
}
