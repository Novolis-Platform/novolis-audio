namespace Novolis.Audio.Voice;

/// <summary>Configuration for <see cref="VoiceService"/>.</summary>
public sealed class VoiceServiceOptions
{
    public VoiceSynthesisOptions Synthesis { get; set; } = new();

    /// <summary>Optional text normalizer (e.g. from Phraseology or ATC profile).</summary>
    public Func<string, string>? NormalizeText { get; set; }
}
