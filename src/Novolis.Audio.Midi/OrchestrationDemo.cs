namespace Novolis.Audio.Midi;

/// <summary>Built-in multi-demo catalog for orchestral score dogfood.</summary>
public sealed record OrchestrationDemo(string Id, string Title, string Blurb, Func<MusicScore> Create);
