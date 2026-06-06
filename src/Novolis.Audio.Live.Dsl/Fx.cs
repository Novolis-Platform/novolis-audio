namespace Novolis.Audio.Live.Dsl;

/// <summary>
/// Named effect aliases for discoverable live coding.
/// </summary>
public static class Fx
{
    public static Novolis.Audio.Live.EffectKind Delay => Novolis.Audio.Live.EffectKind.Delay;
    public static Novolis.Audio.Live.EffectKind Reverb => Novolis.Audio.Live.EffectKind.Reverb;
    public static Novolis.Audio.Live.EffectKind Filter => Novolis.Audio.Live.EffectKind.Filter;
    public static Novolis.Audio.Live.EffectKind Distortion => Novolis.Audio.Live.EffectKind.Distortion;
    public static Novolis.Audio.Live.EffectKind Chorus => Novolis.Audio.Live.EffectKind.Chorus;
    public static Novolis.Audio.Live.EffectKind Compressor => Novolis.Audio.Live.EffectKind.Compressor;
    public static Novolis.Audio.Live.EffectKind Gain => Novolis.Audio.Live.EffectKind.Gain;
}
