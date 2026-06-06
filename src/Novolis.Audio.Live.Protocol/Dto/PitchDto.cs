using MessagePack;

namespace Novolis.Audio.Live.Protocol.Dto;

[MessagePackObject]
public sealed record PitchDto(
    [property: Key(0)] Novolis.Audio.MusicTheory.PitchClass Class,
    [property: Key(1)] int Octave);
