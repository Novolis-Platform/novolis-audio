using Novolis.Audio.Live.Repl;
using Novolis.Audio.MusicTheory;
using Novolis.Audio.Patterns;

namespace Novolis.Audio.Live.Unit;

public sealed class LiveReplSyntaxCompilerTests
{
    [Test]
    public async Task Compile_accepts_note_play_without_arguments()
    {
        var program = new LiveReplSyntaxCompiler().Compile("Note.Play()");

        await Assert.That(program.Bpm).IsEqualTo(120m);
        await Assert.That(program.Tracks.Count).IsEqualTo(1);
        await Assert.That(program.Tracks[0].Pattern is RepeatPattern).IsTrue();
    }

    [Test]
    public async Task Compile_accepts_note_play_with_octave_argument()
    {
        var program = new LiveReplSyntaxCompiler().Compile("Note.Play(3)");
        var repeat = (RepeatPattern)program.Tracks[0].Pattern;
        var note = (NotePattern)repeat.Inner;

        await Assert.That(note.Note.Pitch).IsEqualTo(new Pitch(PitchClass.C, new Octave(3)));
    }

    [Test]
    public async Task Compile_accepts_note_play_with_pitch_token()
    {
        var program = new LiveReplSyntaxCompiler().Compile("Note.Play(F#5);");
        var repeat = (RepeatPattern)program.Tracks[0].Pattern;
        var note = (NotePattern)repeat.Inner;

        await Assert.That(note.Note.Pitch).IsEqualTo(new Pitch(PitchClass.Fs, new Octave(5)));
    }

    [Test]
    public async Task Compile_rejects_unknown_text_shape()
    {
        var act = () => new LiveReplSyntaxCompiler().Compile("Lead.Note(C4)");

        await Assert.That(act).Throws<InvalidOperationException>();
    }
}
