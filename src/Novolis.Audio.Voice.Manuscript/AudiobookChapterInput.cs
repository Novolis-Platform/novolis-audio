namespace Novolis.Audio.Voice.Manuscript;

/// <summary>One chapter input for audiobook generation.</summary>
/// <param name="Id">Stable chapter id used for filenames and manifest.</param>
/// <param name="Title">Human-readable chapter title.</param>
/// <param name="MarkdownPath">Path to chapter markdown on disk.</param>
public sealed record AudiobookChapterInput(string Id, string Title, string MarkdownPath);
