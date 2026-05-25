using Novolis.Audio.Voice;

namespace Novolis.Audio.Unit;

public class VoiceServiceTests
{
    [Test]
    public async Task WriteToFileAsync_writes_valid_wav_header()
    {
        var voice = new VoiceServiceBuilder()
            .UseNullSynthesizer()
            .UseNullPlayback()
            .BuildService();
        var path = Path.Combine(Path.GetTempPath(), $"novolis-voice-{Guid.NewGuid():N}.wav");
        try
        {
            await voice.WriteToFileAsync("Tower, ready for departure.", new FileInfo(path));
            await Assert.That(File.Exists(path)).IsTrue();
            var header = new byte[4];
            await using var stream = File.OpenRead(path);
            _ = await stream.ReadAsync(header);
            await Assert.That(System.Text.Encoding.ASCII.GetString(header)).IsEqualTo("RIFF");
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Test]
    public async Task SpeakAsync_completes_with_null_playback()
    {
        var voice = new VoiceServiceBuilder()
            .UseNullSynthesizer()
            .UseNullPlayback()
            .BuildService();
        await voice.SpeakAsync("SAS one two three, cleared for takeoff runway two two.");
    }
}
