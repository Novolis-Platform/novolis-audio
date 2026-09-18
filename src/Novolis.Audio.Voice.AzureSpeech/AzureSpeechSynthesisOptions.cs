namespace Novolis.Audio.Voice.AzureSpeech;

/// <summary>Azure Speech synthesis settings for one request.</summary>
public sealed record AzureSpeechSynthesisOptions
{
    /// <summary>Azure Speech voice short name.</summary>
    public string VoiceName { get; init; } = "en-US-AvaMultilingualNeural";

    /// <summary>BCP-47 locale used by the SSML document.</summary>
    public string Locale { get; init; } = "en-US";

    /// <summary>Relative speech rate in percent, where zero is the service default.</summary>
    public int RatePercent { get; init; }

    /// <summary>Relative pitch in hertz, where zero is the service default.</summary>
    public int PitchHertz { get; init; }

    /// <summary>Relative volume in percent, where zero is the service default.</summary>
    public int VolumePercent { get; init; }

    /// <summary>Validates the request settings.</summary>
    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(VoiceName);
        ArgumentException.ThrowIfNullOrWhiteSpace(Locale);
        if (RatePercent is < -100 or > 100)
            throw new ArgumentOutOfRangeException(nameof(RatePercent), "Rate must be between -100 and 100 percent.");
        if (PitchHertz is < -1000 or > 1000)
            throw new ArgumentOutOfRangeException(nameof(PitchHertz), "Pitch must be between -1000 and 1000 hertz.");
        if (VolumePercent is < -100 or > 100)
            throw new ArgumentOutOfRangeException(nameof(VolumePercent), "Volume must be between -100 and 100 percent.");
    }
}
