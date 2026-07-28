namespace Novolis.Audio.Voice.EdgeTts;

internal static class EdgeTtsConstants
{
    // Same consumer Read Aloud endpoint used by Microsoft Edge / edge-tts.
    public const string TrustedClientToken = "6A5AA1D4EAFF4E9FB37E23D68491D6F4";
    public const string BasePath = "speech.platform.bing.com/consumer/speech/synthesize/readaloud";

    public const string DefaultVoice = "en-US-AvaNeural";
    public const string ChromiumFullVersion = "143.0.3650.75";

    public static string ChromiumMajorVersion { get; } = ChromiumFullVersion.Split('.')[0];
    public static string SecMsGecVersion { get; } = $"1-{ChromiumFullVersion}";

    public static string WssUrl { get; } =
        $"wss://{BasePath}/edge/v1?TrustedClientToken={TrustedClientToken}";

    public static string VoiceListUrl { get; } =
        $"https://{BasePath}/voices/list?trustedclienttoken={TrustedClientToken}";

    public static string UserAgent { get; } =
        $"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) " +
        $"Chrome/{ChromiumMajorVersion}.0.0.0 Safari/537.36 Edg/{ChromiumMajorVersion}.0.0.0";
}
