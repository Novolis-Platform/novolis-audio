using Novolis.Audio.MusicTheory;

namespace Novolis.Audio.Patterns;

public sealed record NotePattern(Note Note) : PatternNode(PatternNodeKind.Note);
