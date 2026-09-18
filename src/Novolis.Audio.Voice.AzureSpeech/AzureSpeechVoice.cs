namespace Novolis.Audio.Voice.AzureSpeech;

/// <summary>Voice metadata returned by Azure Speech voice discovery.</summary>
public sealed record AzureSpeechVoice(
    string Name,
    string ShortName,
    string Locale,
    string LocalName,
    string Gender,
    IReadOnlyList<string> Styles);
