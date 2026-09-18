using Azure;
using Azure.Core;
using Novolis.Audio.Voice.AzureSpeech;

namespace Novolis.Audio.Unit;

public sealed class AzureSpeechClientTests
{
    static readonly Uri Endpoint = new("https://speech.example.test/");

    [Test]
    public async Task Key_credential_constructs_without_network_access()
    {
        var client = new AzureSpeechClient(Endpoint, new AzureKeyCredential("key"));

        await Assert.That(client.Endpoint).IsEqualTo(Endpoint);
    }

    [Test]
    public async Task Entra_credential_constructs_without_network_access()
    {
        var client = new AzureSpeechClient(Endpoint, new StaticTokenCredential());

        await Assert.That(client.Endpoint).IsEqualTo(Endpoint);
    }

    [Test]
    public async Task Endpoint_must_be_https()
    {
        await Assert.That(() => new AzureSpeechClient(
                new Uri("http://speech.example.test/"),
                new AzureKeyCredential("key")))
            .ThrowsExactly<ArgumentException>();
    }

    [Test]
    public async Task Synthesis_options_validate_bounds()
    {
        await Assert.That(() => new AzureSpeechSynthesisOptions { RatePercent = 101 }.Validate())
            .ThrowsExactly<ArgumentOutOfRangeException>();
        await Assert.That(() => new AzureSpeechSynthesisOptions { PitchHertz = -1001 }.Validate())
            .ThrowsExactly<ArgumentOutOfRangeException>();
        await Assert.That(() => new AzureSpeechSynthesisOptions { VolumePercent = 101 }.Validate())
            .ThrowsExactly<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task Optional_live_synthesis_uses_user_owned_resource()
    {
        var endpointText = Environment.GetEnvironmentVariable("NOVOLIS_AZURE_SPEECH_ENDPOINT");
        var key = Environment.GetEnvironmentVariable("NOVOLIS_AZURE_SPEECH_KEY");
        Skip.Unless(
            Uri.TryCreate(endpointText, UriKind.Absolute, out var endpoint) &&
            string.Equals(endpoint.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(key),
            "Set NOVOLIS_AZURE_SPEECH_ENDPOINT and NOVOLIS_AZURE_SPEECH_KEY to opt into live Azure testing.");

        var client = new AzureSpeechClient(endpoint!, new AzureKeyCredential(key!));
        var mp3 = await client.SynthesizeToMp3Async("Azure Speech integration test.");

        await Assert.That(mp3.Length).IsGreaterThan(128);
        await Assert.That(mp3[0]).IsEqualTo(0xFF);
    }

    sealed class StaticTokenCredential : TokenCredential
    {
        public override AccessToken GetToken(
            TokenRequestContext requestContext,
            CancellationToken cancellationToken) =>
            new("token", DateTimeOffset.UtcNow.AddMinutes(5));

        public override ValueTask<AccessToken> GetTokenAsync(
            TokenRequestContext requestContext,
            CancellationToken cancellationToken) =>
            ValueTask.FromResult(GetToken(requestContext, cancellationToken));
    }
}
