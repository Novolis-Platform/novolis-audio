using System.Text.Json.Serialization;

namespace Novolis.Audio.Voice.EdgeTts;

/// <summary>A voice advertised by Microsoft Edge Read Aloud.</summary>
public sealed class EdgeVoiceInfo
{
    /// <summary>Full Microsoft voice name.</summary>
    [JsonPropertyName("Name")]
    public string Name { get; init; } = "";

    /// <summary>Short id such as <c>en-US-AvaNeural</c>.</summary>
    [JsonPropertyName("ShortName")]
    public string ShortName { get; init; } = "";

    /// <summary>Voice gender label from the catalog.</summary>
    [JsonPropertyName("Gender")]
    public string Gender { get; init; } = "";

    /// <summary>BCP-47 locale.</summary>
    [JsonPropertyName("Locale")]
    public string Locale { get; init; } = "";

    /// <summary>Suggested audio codec string from the catalog.</summary>
    [JsonPropertyName("SuggestedCodec")]
    public string SuggestedCodec { get; init; } = "";

    /// <summary>Human-readable display name.</summary>
    [JsonPropertyName("FriendlyName")]
    public string FriendlyName { get; init; } = "";

    /// <summary>Catalog status (e.g. GA).</summary>
    [JsonPropertyName("Status")]
    public string Status { get; init; } = "";
}
