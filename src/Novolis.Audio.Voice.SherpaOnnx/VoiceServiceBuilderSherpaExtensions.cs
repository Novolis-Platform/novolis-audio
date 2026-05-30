using Novolis.Audio.Core;
using Novolis.Audio.Effects;
using Novolis.Audio.Playback;
using Novolis.Audio.Voice;

namespace Novolis.Audio.Voice.SherpaOnnx;

/// <summary>Sherpa-ONNX extensions for <see cref="VoiceServiceBuilder"/>.</summary>
public static class VoiceServiceBuilderSherpaExtensions
{
    /// <summary>Uses Sherpa-ONNX TTS (falls back to silence when models are absent).</summary>
    public static VoiceServiceBuilder UseSherpaOnnx(this VoiceServiceBuilder builder) =>
        builder.UseSynthesizer(new SherpaVoiceSynthesizer());
}
