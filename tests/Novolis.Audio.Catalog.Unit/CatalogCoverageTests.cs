using System.Net;
using System.Net.Http;
using System.Text;
using Novolis.Audio.Catalog;
using Novolis.Audio.Core;
using Novolis.Audio.Midi;

namespace Novolis.Audio.Catalog.Unit;

public sealed class CatalogCoverageTests
{
    [Test]
    public async Task Media_item_and_download_policy_cover_edge_paths()
    {
        var midi = new MediaItem("a/b c!", "T", "S", MediaKind.Midi, null, MediaLicense.Mutopia, ["x"]);
        await Assert.That(midi.LocalFileName).IsEqualTo("a-b-c-.mid");
        await Assert.That(midi.CanDownload).IsFalse();

        var named = midi with { FileName = "custom.bin", DownloadUrl = "https://www.mutopiaproject.org/x.mid" };
        await Assert.That(named.LocalFileName).IsEqualTo("custom.bin");
        await Assert.That(named.CanDownload).IsTrue();

        await Assert.That(MediaDownloadPolicy.IsDownloadHostAllowed(null)).IsFalse();
        await Assert.That(MediaDownloadPolicy.IsDownloadHostAllowed("")).IsFalse();
        await Assert.That(MediaDownloadPolicy.IsDownloadHostAllowed("not-a-url")).IsFalse();
        await Assert.That(MediaDownloadPolicy.IsDownloadHostAllowed("ftp://example.com/a")).IsFalse();
        await Assert.That(MediaDownloadPolicy.IsDownloadHostAllowed("https://cdn.artlist.io/x")).IsFalse();
        await Assert.That(MediaDownloadPolicy.IsDownloadHostAllowed("https://epidemicsound.com/x")).IsFalse();
        await Assert.That(MediaDownloadPolicy.LooksLikeCommercialInspiration(null)).IsFalse();
        await Assert.That(MediaDownloadPolicy.LooksLikeCommercialInspiration("https://artlist.io/x")).IsTrue();
        await Assert.That(MediaDownloadPolicy.LooksLikeCommercialInspiration("https://example.com/x")).IsFalse();
    }

    [Test]
    public async Task Cache_store_downloads_via_http_handler_and_handles_failures()
    {
        var root = Path.Combine(Path.GetTempPath(), "novolis-cache-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var handler = new ScriptedHandler();
            using var http = new HttpClient(handler);
            var cache = new MediaCacheStore(root, http);

            await Assert.That(cache.RootDirectory).IsEqualTo(root);
            await Assert.That(cache.MidiDirectory).Contains("midi");
            await Assert.That(cache.AudioDirectory).Contains("audio");

            var blocked = new MediaItem(
                "blocked", "B", "Artlist", MediaKind.Audio,
                "https://artlist.io/x", MediaLicense.InspirationCommercial, []);
            await Assert.That(await cache.EnsureCachedAsync(blocked)).IsNull();

            var midiItem = new MediaItem(
                "tiny-mid", "Tiny", "Local", MediaKind.Midi,
                "https://example.com/tiny.mid", MediaLicense.Mutopia, [], FileName: "tiny.mid");
            var midiPath = cache.PathFor(midiItem);
            Directory.CreateDirectory(Path.GetDirectoryName(midiPath)!);
            await File.WriteAllBytesAsync(midiPath, new byte[250]);
            await Assert.That(cache.IsCached(midiItem)).IsTrue();
            await Assert.That(await cache.EnsureCachedAsync(midiItem)).IsEqualTo(midiPath);

            handler.ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(Encoding.ASCII.GetBytes(new string('x', 300))),
            };
            var audioItem = new MediaItem(
                "tone", "Tone", "Local", MediaKind.Audio,
                "https://example.com/tone.wav", MediaLicense.MixkitSfx, [], FileName: "tone.wav");
            var downloaded = await cache.EnsureCachedAsync(audioItem);
            await Assert.That(downloaded).IsNotNull();
            await Assert.That(File.Exists(downloaded!)).IsTrue();

            handler.ResponseFactory = _ => throw new HttpRequestException("boom");
            var failItem = new MediaItem(
                "fail", "Fail", "Local", MediaKind.Audio,
                "https://example.com/fail.wav", MediaLicense.MixkitSfx, [], FileName: "fail.wav");
            await Assert.That(await cache.EnsureCachedAsync(failItem)).IsNull();

            var partialPath = cache.PathFor(failItem);
            await File.WriteAllBytesAsync(partialPath, new byte[250]);
            await Assert.That(await cache.EnsureCachedAsync(failItem)).IsEqualTo(partialPath);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Test]
    public async Task Transformers_decode_midi_sketch_and_pipeline_paths()
    {
        var root = Path.Combine(Path.GetTempPath(), "novolis-xform-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var cache = new MediaCacheStore(root);
            var wavPath = Path.Combine(cache.AudioDirectory, "tone.wav");
            Directory.CreateDirectory(cache.AudioDirectory);
            WriteMonoWav(wavPath, sampleRate: 8_000, frames: 800);

            var audioItem = new MediaItem(
                "tone", "Tone", "Local", MediaKind.Audio,
                "https://example.com/tone.wav", MediaLicense.MixkitSfx, [], FileName: "tone.wav");
            await Assert.That(new DecodePcmTransformer().AppliesTo(audioItem)).IsTrue();
            await Assert.That(new AudioToMidiSketchTransformer().AppliesTo(audioItem)).IsTrue();
            await Assert.That(new DownloadMediaTransformer().AppliesTo(audioItem)).IsTrue();

            var decode = new DecodePcmTransformer();
            await Assert.That(decode.Id).IsEqualTo("decode-pcm");
            await Assert.That(decode.DisplayName.Length).IsGreaterThan(0);
            await Assert.That(decode.Description.Length).IsGreaterThan(0);
            await Assert.That(new DownloadMediaTransformer().Id).IsEqualTo("download");
            await Assert.That(new DownloadMediaTransformer().DisplayName.Length).IsGreaterThan(0);
            await Assert.That(new DownloadMediaTransformer().Description.Length).IsGreaterThan(0);
            await Assert.That(new LoadMidiScoreTransformer().Id).IsEqualTo("load-midi-score");
            await Assert.That(new LoadMidiScoreTransformer().DisplayName.Length).IsGreaterThan(0);
            await Assert.That(new LoadMidiScoreTransformer().Description.Length).IsGreaterThan(0);
            await Assert.That(new AudioToMidiSketchTransformer().Id).IsEqualTo("audio-to-midi-sketch");
            await Assert.That(new AudioToMidiSketchTransformer().DisplayName.Length).IsGreaterThan(0);
            await Assert.That(new AudioToMidiSketchTransformer().Description.Length).IsGreaterThan(0);

            var decodeCtx = new MediaTransformContext { Item = audioItem, Cache = cache, LocalPath = wavPath };
            await new DecodePcmTransformer { SampleRate = 4_000, MaxDuration = TimeSpan.FromSeconds(1) }
                .ApplyAsync(decodeCtx);
            await Assert.That(decodeCtx.Pcm).IsNotNull();
            await Assert.That(decodeCtx.Pcm!.Format.SampleRate).IsEqualTo(4_000);

            var pcmDirect = DecodePcmTransformer.Decode(wavPath, 8_000, TimeSpan.FromSeconds(1));
            await Assert.That(pcmDirect).IsNotNull();
            await Assert.That(DecodePcmTransformer.Decode(wavPath + ".missing", 8_000, TimeSpan.FromSeconds(1)))
                .IsNull();

            var stereoPath = Path.Combine(cache.AudioDirectory, "stereo.wav");
            WriteStereoWav(stereoPath, sampleRate: 8_000, frames: 400);
            await Assert.That(DecodePcmTransformer.Decode(stereoPath, 8_000, TimeSpan.FromSeconds(1))).IsNotNull();
            await Assert.That(DecodePcmTransformer.Decode(stereoPath, 16_000, TimeSpan.FromSeconds(1))).IsNotNull();

            var sketch = new AudioToMidiSketchTransformer();
            var sketchCtx = new MediaTransformContext { Item = audioItem, Cache = cache, LocalPath = wavPath };
            await sketch.ApplyAsync(sketchCtx);
            await Assert.That(sketchCtx.Score).IsNotNull();

            var sketchReady = new MediaTransformContext
            {
                Item = audioItem,
                Cache = cache,
                Pcm = decodeCtx.Pcm,
            };
            await sketch.ApplyAsync(sketchReady);
            await Assert.That(sketchReady.Score).IsNotNull();

            var seq = new MidiSequence("Tiny", 120, 480);
            seq.Add(new MidiNoteEvent(60, 100, TimeSpan.Zero, TimeSpan.FromMilliseconds(200)));
            var midiPath = Path.Combine(cache.MidiDirectory, "tiny.mid");
            Directory.CreateDirectory(cache.MidiDirectory);
            StandardMidiFile.Write(midiPath, seq);

            var midiItem = new MediaItem(
                "tiny", "Tiny MIDI", "Local", MediaKind.Midi,
                "https://www.mutopiaproject.org/tiny.mid", MediaLicense.Mutopia, [], FileName: "tiny.mid");
            var load = new LoadMidiScoreTransformer();
            await Assert.That(load.AppliesTo(midiItem)).IsTrue();
            var midiCtx = new MediaTransformContext { Item = midiItem, Cache = cache, LocalPath = midiPath };
            await load.ApplyAsync(midiCtx);
            await Assert.That(midiCtx.Score).IsNotNull();
            await Assert.That(midiCtx.Score!.Title).IsEqualTo("Tiny MIDI");

            var downloadOk = new DownloadMediaTransformer();
            var handler = new ScriptedHandler
            {
                ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(Encoding.ASCII.GetBytes(new string('y', 300))),
                },
            };
            using var http = new HttpClient(handler);
            var dlCache = new MediaCacheStore(Path.Combine(root, "dl"), http);
            var dlItem = new MediaItem(
                "dl", "Dl", "Local", MediaKind.Audio,
                "https://example.com/dl.wav", MediaLicense.MixkitSfx, [], FileName: "dl.wav");
            var dlCtx = new MediaTransformContext { Item = dlItem, Cache = dlCache };
            await downloadOk.ApplyAsync(dlCtx);
            await Assert.That(dlCtx.LocalPath).IsNotNull();

            var missingCtx = new MediaTransformContext
            {
                Item = audioItem with { DownloadUrl = "https://example.com/missing-never.wav", FileName = "missing-never.wav" },
                Cache = new MediaCacheStore(Path.Combine(root, "miss"), new HttpClient(new ScriptedHandler
                {
                    ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.NotFound),
                })),
            };
            await Assert.That(async () => await downloadOk.ApplyAsync(missingCtx))
                .ThrowsExactly<InvalidOperationException>();
            await Assert.That(async () => await new DecodePcmTransformer().ApplyAsync(
                new MediaTransformContext
                {
                    Item = audioItem with { Id = "uncached-audio", FileName = "uncached-audio.wav" },
                    Cache = new MediaCacheStore(Path.Combine(root, "empty-decode"), new HttpClient(new ScriptedHandler
                    {
                        ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.NotFound),
                    })),
                })).ThrowsExactly<InvalidOperationException>();
            await Assert.That(async () => await new LoadMidiScoreTransformer().ApplyAsync(
                new MediaTransformContext
                {
                    Item = midiItem with { Id = "uncached-midi", FileName = "uncached-midi.mid" },
                    Cache = new MediaCacheStore(Path.Combine(root, "empty-midi"), new HttpClient(new ScriptedHandler
                    {
                        ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.NotFound),
                    })),
                })).ThrowsExactly<InvalidOperationException>();
            await Assert.That(async () => await new AudioToMidiSketchTransformer().ApplyAsync(
                new MediaTransformContext
                {
                    Item = audioItem,
                    Cache = cache,
                    LocalPath = Path.Combine(root, "nope.wav"),
                })).ThrowsExactly<InvalidOperationException>();

            var pipeline = MediaTransformPipeline.DefaultExplore();
            await Assert.That(pipeline.Steps.Count).IsEqualTo(4);
            await Assert.That(pipeline.ApplicableTo(audioItem).Count).IsGreaterThan(0);

            var failPipeline = new MediaTransformPipeline([new ThrowingTransformer()]);
            var failCtx = await failPipeline.RunAsync(audioItem, cache);
            await Assert.That(failCtx.Errors.Count).IsEqualTo(1);
            await Assert.That(failCtx.Ok).IsFalse();

            var filtered = await pipeline.RunAsync(audioItem, cache, onlyTransformerIds: ["nope"]);
            await Assert.That(filtered.Log.Count).IsEqualTo(0);

            var download = new DownloadMediaTransformer();
            var commercial = new MediaItem(
                "c", "C", "Artlist", MediaKind.Audio,
                "https://artlist.io/x", MediaLicense.InspirationCommercial, []);
            // AppliesTo is false for commercial; force ApplyAsync throw path via LooksLikeCommercialInspiration
            // when CanDownload is somehow true is unreachable — exercise throw via a downloadable-looking
            // commercial host with AllowsDownload license (policy still blocks CanDownload).
            await Assert.That(download.AppliesTo(commercial)).IsFalse();

            var downloadBlockedCtx = new MediaTransformContext
            {
                Item = commercial with { License = MediaLicense.Mutopia },
                Cache = cache,
            };
            // Mutopia + artlist host => CanDownload false, but ApplyAsync still checks LooksLikeCommercialInspiration
            await Assert.That(async () => await download.ApplyAsync(downloadBlockedCtx))
                .ThrowsExactly<InvalidOperationException>();
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Test]
    public async Task Hub_search_explore_and_inspiration_branches()
    {
        var root = Path.Combine(Path.GetTempPath(), "novolis-hub-" + Guid.NewGuid().ToString("N"));
        var hub = MediaCatalogHub.CreateDefault(root);
        try
        {
            await Assert.That(hub.Sources.Count).IsEqualTo(2);
            await Assert.That(hub.Inspiration).IsNotNull();
            await Assert.That(hub.FindCollection("missing-id")).IsNull();
            await Assert.That(await hub.FindCollectionAsync("missing-id")).IsNull();

            var all = await hub.ListAllCollectionsAsync();
            await Assert.That(all.Count).IsGreaterThan(0);

            var byQuery = hub.Search("bach", mood: null);
            await Assert.That(byQuery.Count).IsGreaterThan(0);

            var byBoth = hub.Search("pathétique", mood: "heroic");
            await Assert.That(byBoth.Count).IsGreaterThan(0);

            var item = hub.FindCollection("mutopia-classical")!.Items[0];
            var explore = await hub.ExploreAsync(item, transformerIds: ["download"]);
            await Assert.That(explore.Item.Id).IsEqualTo(item.Id);

            var (bookmark, standIn) = hub.AddInspiration(
                new Uri("https://musicbed.com/collection/ambient-night-cinematic-space-board-with-a-very-long-path-that-should-truncate-in-the-bookmark-title-when-needed"),
                title: null);
            await Assert.That(standIn).IsNotNull();
            await Assert.That(bookmark.Title.Length).IsLessThanOrEqualTo(64);
            await Assert.That(hub.Inspiration!.GetCollectionAsync(bookmark.Id).Result).IsNotNull();

            var (generic, free) = hub.AddInspiration(new Uri("https://premiumbeat.com/moods/chill"), "chill");
            await Assert.That(free!.Id).IsEqualTo("mixkit-sfx");
            await Assert.That(generic.Moods.Contains("inspiration")).IsTrue();

            var inspirationOnly = new InspirationBookmarkSource();
            await Assert.That(() => inspirationOnly.AddOrUpdate(new Uri("https://example.com/ok")))
                .ThrowsExactly<ArgumentException>();

            var bareHub = new MediaCatalogHub([new CuratedFreeCatalogSource()], new MediaCacheStore(root));
            await Assert.That(() => bareHub.AddInspiration(new Uri("https://artlist.io/x")))
                .ThrowsExactly<InvalidOperationException>();
            await Assert.That(bareHub.Cache.RootDirectory).IsEqualTo(root);
            await Assert.That(new MediaCollection(
                "c", "t", "d", "s", [item], ["mood"]).Count).IsEqualTo(1);

            var defaultCache = new MediaCacheStore();
            await Assert.That(defaultCache.RootDirectory).Contains("MediaCatalog");
            await Assert.That(new MediaTransformContext { Item = item, Cache = defaultCache }.Ok).IsTrue();
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    static void WriteMonoWav(string path, int sampleRate, int frames)
    {
        WriteWav(path, sampleRate, frames, channels: 1);
    }

    static void WriteStereoWav(string path, int sampleRate, int frames)
    {
        WriteWav(path, sampleRate, frames, channels: 2);
    }

    static void WriteWav(string path, int sampleRate, int frames, int channels)
    {
        var dataBytes = frames * channels * 2;
        using var fs = File.Create(path);
        using var bw = new BinaryWriter(fs);
        bw.Write(Encoding.ASCII.GetBytes("RIFF"));
        bw.Write(36 + dataBytes);
        bw.Write(Encoding.ASCII.GetBytes("WAVE"));
        bw.Write(Encoding.ASCII.GetBytes("fmt "));
        bw.Write(16);
        bw.Write((short)1);
        bw.Write((short)channels);
        bw.Write(sampleRate);
        bw.Write(sampleRate * channels * 2);
        bw.Write((short)(channels * 2));
        bw.Write((short)16);
        bw.Write(Encoding.ASCII.GetBytes("data"));
        bw.Write(dataBytes);
        for (var i = 0; i < frames; i++)
        {
            var sample = (short)(Math.Sin(2 * Math.PI * 440 * i / sampleRate) * 10_000);
            for (var c = 0; c < channels; c++)
                bw.Write(sample);
        }
    }

    sealed class ScriptedHandler : HttpMessageHandler
    {
        public Func<HttpRequestMessage, HttpResponseMessage> ResponseFactory { get; set; } =
            _ => new HttpResponseMessage(HttpStatusCode.OK);

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(ResponseFactory(request));
    }

    sealed class ThrowingTransformer : IMediaTransformer
    {
        public string Id => "throw";
        public string DisplayName => "Throw";
        public string Description => "Always fails";
        public bool AppliesTo(MediaItem item) => true;
        public ValueTask ApplyAsync(MediaTransformContext context, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("intentional");
    }
}
