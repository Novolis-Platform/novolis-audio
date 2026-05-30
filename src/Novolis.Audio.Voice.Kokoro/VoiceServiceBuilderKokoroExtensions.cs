using Novolis.Audio.Voice;

namespace Novolis.Audio.Voice.Kokoro;

/// <summary>Kokoro extensions for <see cref="VoiceServiceBuilder"/>.</summary>
public static class VoiceServiceBuilderKokoroExtensions
{
    /// <summary>Uses Kokoro ONNX TTS (silent fallback when the model is absent).</summary>
    public static VoiceServiceBuilder UseKokoro(this VoiceServiceBuilder builder) =>
        builder.UseSynthesizer(new KokoroVoiceSynthesizer());
}
