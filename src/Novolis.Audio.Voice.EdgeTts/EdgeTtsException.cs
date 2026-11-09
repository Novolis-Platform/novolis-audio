namespace Novolis.Audio.Voice.EdgeTts;

/// <summary>Failure talking to Microsoft Edge Read Aloud TTS.</summary>
public sealed class EdgeTtsException : Exception
{
    /// <summary>Creates an exception with a message.</summary>
    public EdgeTtsException(string message) : base(message)
    {
    }

    /// <summary>Creates an exception with a message and inner exception.</summary>
    public EdgeTtsException(string message, Exception inner) : base(message, inner)
    {
    }
}
