namespace Novolis.Audio.Voice.Profiles;

/// <summary>Neutral base voice: Piper model + synthesis temperament. No DSP or phraseology.</summary>
/// <param name="Profile">Stable archetype id (e.g. <c>excitable_female</c>).</param>
/// <param name="Model">Bundled Sherpa/Piper model profile.</param>
/// <param name="SpeakingRate">Speaking rate multiplier (&gt;1 = faster).</param>
/// <param name="Description">Human-readable character summary.</param>
public sealed record VoiceArchetype(
    VoiceProfile Profile,
    VoiceModelProfile Model,
    float SpeakingRate,
    string Description);
