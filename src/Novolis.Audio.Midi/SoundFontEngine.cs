using System.Net.Http;
using MeltySynth;
using Novolis.Audio.Core;

namespace Novolis.Audio.Midi;

/// <summary>
/// MeltySynth SoundFont host. Prefers a real GM bank (TimGM6mb) over parametric oscillators.
/// Caches under %LOCALAPPDATA%/Novolis/SoundFonts/ when needed.
/// </summary>
public static class SoundFontEngine
{
    static readonly object Gate = new();
    static SoundFont? _font;
    static string? _path;
    static string? _error;
    static readonly HttpClient Http = new() { Timeout = TimeSpan.FromMinutes(3) };

    public const string DefaultFileName = "TimGM6mb.sf2";
    public const string DefaultDownloadUrl =
        "https://archive.org/download/free-soundfonts-sf2-2019-04/TimGM6mb.sf2";

    /// <summary>True when a SoundFont is loaded and ready.</summary>
    public static bool IsAvailable
    {
        get
        {
            try
            {
                EnsureLoaded(downloadIfMissing: false);
                return _font is not null;
            }
            catch (Exception ex) when (ex is FileNotFoundException or TypeLoadException or DllNotFoundException)
            {
                _error = ex.Message;
                return false;
            }
        }
    }

    public static string? LoadedPath => _path;
    public static string? LastError => _error;

    /// <summary>Force parametric path (tests / debugging).</summary>
    public static bool ForceParametric
    {
        get => string.Equals(
            Environment.GetEnvironmentVariable("NOVOLIS_MIDI_FORCE_PARAMETRIC"),
            "1",
            StringComparison.Ordinal);
        set => Environment.SetEnvironmentVariable(
            "NOVOLIS_MIDI_FORCE_PARAMETRIC",
            value ? "1" : "");
    }

    /// <summary>Ensures TimGM6mb (or NOVOLIS_SOUNDFONT) is on disk and loaded.</summary>
    public static bool EnsureInstalled(bool downloadIfMissing = true)
    {
        if (ForceParametric)
            return false;
        try
        {
            EnsureLoaded(downloadIfMissing);
            return _font is not null;
        }
        catch (Exception ex) when (ex is FileNotFoundException or TypeLoadException or DllNotFoundException)
        {
            _error = ex.Message;
            return false;
        }
    }

    /// <summary>Renders one note via SoundFont, or null to fall back.</summary>
    public static PcmBuffer? TryRenderNote(
        PcmFormat format,
        InstrumentPatch patch,
        int midiNumber,
        TimeSpan holdDuration,
        int velocity)
    {
        if (ForceParametric || !EnsureInstalled())
            return null;

        try
        {
            return RenderNoteCore(format, patch, midiNumber, holdDuration, velocity);
        }
        catch (Exception ex) when (ex is FileNotFoundException or TypeLoadException or DllNotFoundException)
        {
            _error = ex.Message;
            return null;
        }
    }

    static PcmBuffer? RenderNoteCore(
        PcmFormat format,
        InstrumentPatch patch,
        int midiNumber,
        TimeSpan holdDuration,
        int velocity)
    {
        lock (Gate)
        {
            if (_font is null)
                return null;

            try
            {
                var sr = format.SampleRate;
                var synth = new Synthesizer(_font, sr);
                ConfigurePatch(synth, patch, midiNumber, out var channel, out var key);
                velocity = Math.Clamp(velocity, 1, 127);
                var hold = Math.Max(0.04, holdDuration.TotalSeconds);
                var releasePad = 0.55;
                var total = hold + releasePad;
                var frames = Math.Max(1, (int)(sr * total));
                var left = new float[frames];
                var right = new float[frames];

                var block = Math.Max(64, synth.BlockSize);
                var onFrames = (int)(hold * sr);
                var written = 0;
                var noteOn = false;
                var noteOff = false;

                while (written < frames)
                {
                    var n = Math.Min(block, frames - written);
                    if (!noteOn)
                    {
                        synth.NoteOn(channel, key, velocity);
                        noteOn = true;
                    }

                    if (!noteOff && written >= onFrames)
                    {
                        synth.NoteOff(channel, key);
                        noteOff = true;
                    }

                    synth.Render(left.AsSpan(written, n), right.AsSpan(written, n));
                    written += n;
                }

                if (!noteOff)
                    synth.NoteOff(channel, key);

                // Mono mix + gentle gain (SoundFonts are often hot).
                var mono = new float[frames];
                var peak = 1e-6f;
                for (var i = 0; i < frames; i++)
                {
                    var s = (left[i] + right[i]) * 0.5f * patch.Gain * 1.35f;
                    mono[i] = s;
                    peak = Math.Max(peak, Math.Abs(s));
                }

                if (peak > 0.95f)
                {
                    var scale = 0.92f / peak;
                    for (var i = 0; i < frames; i++)
                        mono[i] *= scale;
                }

                return ToPcm(format, mono);
            }
            catch (Exception ex)
            {
                _error = ex.Message;
                return null;
            }
        }
    }

    /// <summary>Renders a full multi-track score with SoundFont instruments.</summary>
    public static PcmBuffer? TryRenderScore(PcmFormat format, InstrumentBank bank, MusicScore score)
    {
        if (ForceParametric || !EnsureInstalled())
            return null;

        lock (Gate)
        {
            if (_font is null)
                return null;

            try
            {
                score.EnsureDefaultTrack();
                var endBeat = Math.Max(score.TotalBeats, score.ContentEndBeat);
                var duration = TimeSpan.FromMinutes(endBeat / Math.Max(40, score.TempoBpm)) + TimeSpan.FromSeconds(1.2);
                var sr = format.SampleRate;
                var frames = Math.Max(1, (int)(sr * duration.TotalSeconds));
                var left = new float[frames];
                var right = new float[frames];
                var synth = new Synthesizer(_font, sr);
                var block = Math.Max(64, synth.BlockSize);

                var anySolo = score.Tracks.Any(t => t.Solo);
                var events = new List<(int Frame, bool On, int Channel, int Key, int Velocity, int Program, bool Drum)>();

                var channel = 0;
                foreach (var track in score.Tracks)
                {
                    if (track.Mute || (anySolo && !track.Solo))
                        continue;
                    var patch = bank.Find(track.PatchId) ?? bank.Patches[0];
                    var program = GmProgramMap.TryGetProgram(patch.Id);
                    var drum = program is null;
                    var ch = drum ? 9 : channel % 9; // reserve 9 for drums; melodic 0–8
                    if (!drum)
                        channel++;

                    foreach (var note in score.Notes.Where(n => n.TrackId == track.Id))
                    {
                        var start = (int)(TimeSpan.FromMinutes(note.StartBeat / score.TempoBpm).TotalSeconds * sr);
                        var end = start + Math.Max(1, (int)(TimeSpan.FromMinutes(note.DurationBeats / score.TempoBpm).TotalSeconds * sr));
                        var key = drum
                            ? (note.MidiNumber is >= 35 and <= 81
                                ? note.MidiNumber
                                : GmProgramMap.DrumKey(patch.Id, note.MidiNumber))
                            : note.MidiNumber;
                        var prog = program ?? 0;
                        events.Add((start, true, ch, key, note.Velocity, prog, drum));
                        events.Add((end, false, ch, key, 0, prog, drum));
                    }
                }

                events.Sort((a, b) =>
                {
                    var c = a.Frame.CompareTo(b.Frame);
                    if (c != 0)
                        return c;
                    // NoteOff before NoteOn at same frame
                    return a.On.CompareTo(b.On);
                });

                var ei = 0;
                var programs = new int[16];
                Array.Fill(programs, -1);

                for (var written = 0; written < frames;)
                {
                    var n = Math.Min(block, frames - written);
                    while (ei < events.Count && events[ei].Frame <= written)
                    {
                        var e = events[ei++];
                        if (!e.Drum && programs[e.Channel] != e.Program)
                        {
                            synth.ProcessMidiMessage(e.Channel, 0xC0, e.Program, 0);
                            programs[e.Channel] = e.Program;
                        }

                        if (e.On)
                            synth.NoteOn(e.Channel, e.Key, Math.Clamp(e.Velocity, 1, 127));
                        else
                            synth.NoteOff(e.Channel, e.Key);
                    }

                    synth.Render(left.AsSpan(written, n), right.AsSpan(written, n));
                    written += n;
                }

                var mono = new float[frames];
                var peak = 1e-6f;
                for (var i = 0; i < frames; i++)
                {
                    var s = (left[i] + right[i]) * 0.42f;
                    mono[i] = s;
                    peak = Math.Max(peak, Math.Abs(s));
                }

                if (peak > 0.95f)
                {
                    var scale = 0.9f / peak;
                    for (var i = 0; i < frames; i++)
                        mono[i] *= scale;
                }

                return ToPcm(format, mono);
            }
            catch (Exception ex)
            {
                _error = ex.Message;
                return null;
            }
        }
    }

    static void ConfigurePatch(Synthesizer synth, InstrumentPatch patch, int midiNumber, out int channel, out int key)
    {
        var program = GmProgramMap.TryGetProgram(patch.Id);
        if (program is null)
        {
            channel = 9;
            key = midiNumber is >= 35 and <= 81
                ? midiNumber
                : GmProgramMap.DrumKey(patch.Id, midiNumber);
            return;
        }

        channel = 0;
        key = midiNumber;
        synth.ProcessMidiMessage(0, 0xC0, program.Value, 0);
    }

    static void EnsureLoaded(bool downloadIfMissing)
    {
        if (ForceParametric)
            return;

        lock (Gate)
        {
            if (_font is not null)
                return;

            try
            {
                var path = ResolveExistingPath() ?? (downloadIfMissing ? DownloadDefault() : null);
                if (path is null || !File.Exists(path))
                {
                    _error ??= "No SoundFont found. Set NOVOLIS_SOUNDFONT or allow TimGM6mb download.";
                    return;
                }

                _font = new SoundFont(path);
                _path = path;
                _error = null;
            }
            catch (Exception ex)
            {
                _font = null;
                _path = null;
                _error = ex.Message;
            }
        }
    }

    static string? ResolveExistingPath()
    {
        var env = Environment.GetEnvironmentVariable("NOVOLIS_SOUNDFONT");
        if (!string.IsNullOrWhiteSpace(env) && File.Exists(env))
            return env;

        var cache = CachePath();
        if (File.Exists(cache))
            return cache;

        // Dogfood / app content next to entry assembly
        var baseDir = AppContext.BaseDirectory;
        foreach (var name in new[] { DefaultFileName, "GeneralUser.sf2", "FluidR3_GM.sf2" })
        {
            var p = Path.Combine(baseDir, name);
            if (File.Exists(p))
                return p;
            p = Path.Combine(baseDir, "SoundFonts", name);
            if (File.Exists(p))
                return p;
        }

        return null;
    }

    static string CachePath() =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Novolis",
            "SoundFonts",
            DefaultFileName);

    static string? DownloadDefault()
    {
        var dest = CachePath();
        Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
        var tmp = dest + ".partial";
        try
        {
            using var response = Http.GetAsync(DefaultDownloadUrl).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            using var stream = response.Content.ReadAsStream();
            using var file = File.Create(tmp);
            stream.CopyTo(file);
            file.Flush();
            file.Dispose();
            if (File.Exists(dest))
                File.Delete(dest);
            File.Move(tmp, dest);
            return dest;
        }
        catch (Exception ex)
        {
            _error = $"SoundFont download failed: {ex.Message}";
            try
            {
                if (File.Exists(tmp))
                    File.Delete(tmp);
            }
            catch
            {
                // ignore
            }

            return File.Exists(dest) ? dest : null;
        }
    }

    static PcmBuffer ToPcm(PcmFormat format, float[] samples)
    {
        var bytes = new byte[samples.Length * 2];
        for (var i = 0; i < samples.Length; i++)
        {
            var v = Math.Clamp(samples[i], -1f, 1f);
            System.Buffers.Binary.BinaryPrimitives.WriteInt16LittleEndian(
                bytes.AsSpan(i * 2),
                (short)(v * short.MaxValue));
        }

        return new PcmBuffer(format, bytes, samples.Length);
    }
}
