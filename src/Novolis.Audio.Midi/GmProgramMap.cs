namespace Novolis.Audio.Midi;

/// <summary>Maps Novolis patch ids to General MIDI program numbers (0–127).</summary>
public static class GmProgramMap
{
    static readonly Dictionary<string, int> Programs = new(StringComparer.OrdinalIgnoreCase)
    {
        ["keys.grand-soft"] = 0,
        ["keys.bright-piano"] = 1,
        ["keys.electric"] = 4,
        ["keys.clav"] = 7,
        ["keys.harpsichord"] = 6,
        ["keys.pipe-organ"] = 19,
        ["keys.reed-organ"] = 20,
        ["keys.accordion"] = 21,
        ["keys.celesta"] = 8,

        ["lead.soft-sine"] = 80,
        ["lead.square"] = 80,
        ["lead.saw"] = 81,
        ["lead.pulse"] = 82,
        ["lead.brass"] = 62,
        ["lead.choir-ah"] = 52,
        ["lead.supersaw"] = 81,
        ["lead.fifth"] = 86,

        ["bass.sub"] = 38,
        ["bass.finger"] = 33,
        ["bass.acid"] = 38,
        ["bass.reese"] = 39,
        ["bass.pluck"] = 34,
        ["bass.square"] = 38,

        ["pad.warm"] = 89,
        ["pad.glass"] = 98,
        ["pad.strings"] = 48,
        ["pad.choir"] = 52,
        ["pad.analog"] = 95,
        ["pad.night"] = 89,

        ["pluck.nylon"] = 24,
        ["pluck.steel"] = 25,
        ["pluck.mandolin"] = 32,
        ["pluck.kalimba"] = 108,
        ["pluck.harp"] = 46,
        ["pluck.banjo"] = 105,

        ["bell.tubular"] = 14,
        ["bell.fm"] = 98,
        ["bell.glock"] = 9,
        ["bell.crystal"] = 98,
        ["bell.marimba"] = 12,
        ["bell.vibraphone"] = 11,

        ["brass.trumpet"] = 56,
        ["brass.horn"] = 60,
        ["brass.synth"] = 62,
        ["brass.trombone"] = 57,
        ["orch.timpani"] = 47,
        ["wind.flute"] = 73,
        ["wind.clarinet"] = 71,
        ["wind.oboe"] = 68,
        ["wind.pan"] = 75,
    };

    static readonly Dictionary<string, int> DrumKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        ["perc.kick"] = 36,
        ["perc.snare"] = 38,
        ["perc.hat-closed"] = 42,
        ["perc.hat-open"] = 46,
        ["perc.tom"] = 45,
        ["perc.clap"] = 39,
        ["perc.ride"] = 51,
    };

    /// <summary>Returns a GM program for <paramref name="patchId"/>, or null for drum kit routing.</summary>
    public static int? TryGetProgram(string patchId)
    {
        if (string.IsNullOrWhiteSpace(patchId))
            return 0;

        if (patchId.StartsWith("perc.", StringComparison.OrdinalIgnoreCase))
            return null; // channel 9 drum kit

        if (Programs.TryGetValue(patchId, out var program))
            return program;

        return Infer(patchId);
    }

    /// <summary>GM drum-key for percussion patch ids (channel 9).</summary>
    public static int DrumKey(string patchId, int fallbackMidi) =>
        DrumKeys.TryGetValue(patchId, out var key)
            ? key
            : Math.Clamp(fallbackMidi, 35, 81);

    static int Infer(string patchId)
    {
        var p = patchId.ToLowerInvariant();
        if (p.Contains("timpani")) return 47;
        if (p.Contains("trombone")) return 57;
        if (p.Contains("piano") || p.Contains("keys.")) return 0;
        if (p.Contains("bass")) return 33;
        if (p.Contains("string")) return 48;
        if (p.Contains("brass") || p.Contains("trumpet")) return 56;
        if (p.Contains("flute") || p.Contains("wind")) return 73;
        if (p.Contains("guitar") || p.Contains("pluck")) return 25;
        if (p.Contains("organ")) return 19;
        if (p.Contains("pad")) return 89;
        if (p.Contains("lead") || p.Contains("saw")) return 81;
        if (p.Contains("bell")) return 14;
        return 0;
    }
}
