using Novolis.Audio.Core;

namespace Novolis.Audio.Edit;

/// <summary>Exports the arrangement mixdown as a WAV file.</summary>
public static class ArrangementExporter
{
    public static string ExportWav(MusicProject project, string path)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var mix = ArrangementMixer.Render(project);
        new WavEncoder().EncodeFile(mix, path);
        return path;
    }
}
