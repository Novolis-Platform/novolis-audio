using Novolis.Audio.Live.Dsl;
using Novolis.Audio.MusicTheory;

namespace Novolis.Audio.Live.Unit;

public sealed class LiveDslInstrumentsTests
{
    [Test]
    public async Task Instruments_aliases_match_music_theory_kinds()
    {
        await Assert.That(Instruments.Lead).IsEqualTo(InstrumentKind.Lead);
        await Assert.That(Instruments.Bass).IsEqualTo(InstrumentKind.Bass);
        await Assert.That(Instruments.Pad).IsEqualTo(InstrumentKind.Pad);
        await Assert.That(Instruments.Pluck).IsEqualTo(InstrumentKind.Pluck);
        await Assert.That(Instruments.Bell).IsEqualTo(InstrumentKind.Bell);
        await Assert.That(Instruments.Keys).IsEqualTo(InstrumentKind.Keys);
        await Assert.That(Instruments.Kick).IsEqualTo(InstrumentKind.Kick);
        await Assert.That(Instruments.Snare).IsEqualTo(InstrumentKind.Snare);
        await Assert.That(Instruments.Hat).IsEqualTo(InstrumentKind.Hat);
        await Assert.That(Instruments.Clap).IsEqualTo(InstrumentKind.Clap);
        await Assert.That(Instruments.Tom).IsEqualTo(InstrumentKind.Tom);
        await Assert.That(Instruments.Noise).IsEqualTo(InstrumentKind.Noise);
        await Assert.That(Instruments.Sampler).IsEqualTo(InstrumentKind.Sampler);
    }

    [Test]
    public async Task Fx_aliases_match_effect_kinds()
    {
        await Assert.That(Fx.Delay).IsEqualTo(Novolis.Audio.Live.EffectKind.Delay);
        await Assert.That(Fx.Reverb).IsEqualTo(Novolis.Audio.Live.EffectKind.Reverb);
        await Assert.That(Fx.Filter).IsEqualTo(Novolis.Audio.Live.EffectKind.Filter);
        await Assert.That(Fx.Distortion).IsEqualTo(Novolis.Audio.Live.EffectKind.Distortion);
        await Assert.That(Fx.Chorus).IsEqualTo(Novolis.Audio.Live.EffectKind.Chorus);
        await Assert.That(Fx.Compressor).IsEqualTo(Novolis.Audio.Live.EffectKind.Compressor);
        await Assert.That(Fx.Gain).IsEqualTo(Novolis.Audio.Live.EffectKind.Gain);
    }

    [Test]
    public async Task LiveDsl_chord_layer_transpose_build_pattern_nodes()
    {
        var chord = LiveDsl.Chord(PitchClass.A, Octave.MiddleC, ChordQuality.Minor, Duration.Quarter);
        var layer = LiveDsl.Layer(chord, LiveDsl.Rest(Duration.Eighth));
        var transposed = LiveDsl.Transpose(layer, semitones: 2);

        await Assert.That(chord.Chord.Quality).IsEqualTo(ChordQuality.Minor);
        await Assert.That(layer.Layers.Count).IsEqualTo(2);
        await Assert.That(transposed.Semitones).IsEqualTo(2);
    }
}
