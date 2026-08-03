namespace Novolis.Audio.Midi;

/// <summary>Built-in multi-demo catalog for orchestral score dogfood.</summary>
public sealed record OrchestrationDemo(string Id, string Title, string Blurb, Func<MusicScore> Create);

/// <summary>Named score demos (original Novolis writing + loadable free MIDI).</summary>
public static class OrchestrationDemoCatalog
{
    public static IReadOnlyList<OrchestrationDemo> All { get; } =
    [
        new(
            "orbital-fanfare",
            "Orbital Fanfare · Kick/Tom Remix",
            "Original ~20s cinematic brass + martial drums.",
            MusicScore.CreateCinematicFanfare),
        new(
            "autumn-cadence",
            "Autumn Cadence",
            "Piano / bass / lead jazz-pop cadence.",
            MusicScore.CreateDemo),
        new(
            "ember-steel",
            "Ember Steel Overture",
            "Original hybrid trailer sketch (not a licensed theme).",
            MusicScore.CreateEmberSteelOverture),
        new(
            "string-adagio",
            "Northern Adagio",
            "Lush string pad chorale with soft brass.",
            MusicScore.CreateStringAdagio),
        new(
            "marching-brass",
            "Iron Parade",
            "4/4 marching brass + snare cadence.",
            MusicScore.CreateMarchingBrass),
        new(
            "waltz-trio",
            "Harbor Waltz",
            "3/4 piano trio waltz.",
            MusicScore.CreateWaltzTrio),
    ];

    public static OrchestrationDemo? Find(string id) =>
        All.FirstOrDefault(d => string.Equals(d.Id, id, StringComparison.OrdinalIgnoreCase));
}
