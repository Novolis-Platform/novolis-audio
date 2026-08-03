namespace Novolis.Audio.Midi;

/// <summary>Staff clef for an orchestral part.</summary>
public enum ScoreClef
{
    /// <summary>G clef (violin, flute, piano right hand, leads).</summary>
    Treble,

    /// <summary>F clef (cello, bass, trombone, piano left hand).</summary>
    Bass,

    /// <summary>C clef on the middle line (viola).</summary>
    Alto,

    /// <summary>Linked treble + bass staves (keyboard / harp).</summary>
    Grand,
}
