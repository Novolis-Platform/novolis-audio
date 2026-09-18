namespace Novolis.Audio.Voice.AzureSpeech;

/// <summary>Describes an Azure Speech request or service failure without including input text.</summary>
public sealed class AzureSpeechException : Exception
{
    /// <summary>Creates an exception with a safe diagnostic message.</summary>
    public AzureSpeechException(string message)
        : base(message)
    {
    }

    /// <summary>Creates an exception with a safe diagnostic message and an underlying exception.</summary>
    public AzureSpeechException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>Azure cancellation error code, when the service supplied one.</summary>
    public string? ErrorCode { get; init; }
}
