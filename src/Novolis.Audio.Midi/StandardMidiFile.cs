using System.Buffers.Binary;
using System.Text;

namespace Novolis.Audio.Midi;

/// <summary>Minimal Standard MIDI File (Type 0) reader/writer for note sequences.</summary>
public static class StandardMidiFile
{
    /// <summary>Writes a Type-0 SMF for <paramref name="sequence"/>.</summary>
    public static void Write(string path, MidiSequence sequence)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(sequence);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllBytes(path, WriteBytes(sequence));
    }

    /// <summary>Serializes to SMF bytes.</summary>
    public static byte[] WriteBytes(MidiSequence sequence)
    {
        ArgumentNullException.ThrowIfNull(sequence);

        var events = new List<(int Tick, byte[] Data)>();
        var tempo = (int)Math.Round(60_000_000 / sequence.TempoBpm);
        events.Add((0, [(byte)0xFF, 0x51, 0x03, (byte)(tempo >> 16), (byte)(tempo >> 8), (byte)tempo]));

        if (!string.IsNullOrWhiteSpace(sequence.Title))
        {
            var titleBytes = Encoding.ASCII.GetBytes(sequence.Title);
            if (titleBytes.Length > 120)
                titleBytes = titleBytes.AsSpan(0, 120).ToArray();
            var meta = new byte[3 + titleBytes.Length];
            meta[0] = 0xFF;
            meta[1] = 0x03;
            meta[2] = (byte)titleBytes.Length;
            titleBytes.CopyTo(meta.AsSpan(3));
            events.Add((0, meta));
        }

        foreach (var note in sequence.Notes)
        {
            var onTick = Math.Max(0, sequence.SecondsToTicks(note.Start));
            var offTick = Math.Max(onTick + 1, sequence.SecondsToTicks(note.End));
            events.Add((onTick, [0x90, (byte)note.MidiNumber, (byte)note.Velocity]));
            events.Add((offTick, [0x80, (byte)note.MidiNumber, 0x40]));
        }

        events.Sort((a, b) => a.Tick.CompareTo(b.Tick));

        using var track = new MemoryStream();
        var lastTick = 0;
        foreach (var (tick, data) in events)
        {
            WriteVarLen(track, tick - lastTick);
            track.Write(data);
            lastTick = tick;
        }

        WriteVarLen(track, 0);
        track.Write([0xFF, 0x2F, 0x00]); // end of track

        var trackBytes = track.ToArray();
        using var smf = new MemoryStream();
        smf.Write("MThd"u8);
        WriteBe32(smf, 6);
        WriteBe16(smf, 0); // type 0
        WriteBe16(smf, 1); // one track
        WriteBe16(smf, (ushort)sequence.TicksPerQuarter);
        smf.Write("MTrk"u8);
        WriteBe32(smf, trackBytes.Length);
        smf.Write(trackBytes);
        return smf.ToArray();
    }

    /// <summary>Loads notes from a Type-0/1 SMF (note on/off + tempo).</summary>
    public static MidiSequence Read(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return ReadBytes(File.ReadAllBytes(path), Path.GetFileNameWithoutExtension(path));
    }

    /// <summary>Parses SMF bytes into a sequence.</summary>
    public static MidiSequence ReadBytes(ReadOnlySpan<byte> data, string title = "Imported")
    {
        if (data.Length < 14 || !data.StartsWith("MThd"u8))
            throw new InvalidDataException("Not a Standard MIDI File.");

        var headerLen = BinaryPrimitives.ReadInt32BigEndian(data.Slice(4, 4));
        var format = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(8, 2));
        var trackCount = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(10, 2));
        var division = BinaryPrimitives.ReadInt16BigEndian(data.Slice(12, 2));
        if (division <= 0)
            throw new InvalidDataException("SMPTE MIDI timing is not supported.");

        var sequence = new MidiSequence(title, tempoBpm: 120, ticksPerQuarter: division);
        var offset = 8 + headerLen;
        var openNotes = new Dictionary<(int Track, int Note), (int Tick, int Velocity)>();
        var tempoBpm = 120.0;

        for (var track = 0; track < trackCount && offset + 8 <= data.Length; track++)
        {
            if (!data.Slice(offset).StartsWith("MTrk"u8))
                throw new InvalidDataException("Expected MTrk chunk.");
            var trackLen = BinaryPrimitives.ReadInt32BigEndian(data.Slice(offset + 4, 4));
            offset += 8;
            var trackEnd = offset + trackLen;
            var tick = 0;
            byte runningStatus = 0;

            while (offset < trackEnd)
            {
                tick += ReadVarLen(data, ref offset);
                if (offset >= trackEnd)
                    break;

                var status = data[offset];
                if (status < 0x80)
                {
                    if (runningStatus == 0)
                        throw new InvalidDataException("Missing MIDI running status.");
                    status = runningStatus;
                }
                else
                {
                    offset++;
                    if (status < 0xF0)
                        runningStatus = status;
                }

                if (status == 0xFF)
                {
                    if (offset + 1 >= trackEnd)
                        break;
                    var metaType = data[offset++];
                    var metaLen = ReadVarLen(data, ref offset);
                    var metaData = data.Slice(offset, metaLen);
                    offset += metaLen;
                    if (metaType == 0x51 && metaLen == 3)
                    {
                        var micros = (metaData[0] << 16) | (metaData[1] << 8) | metaData[2];
                        if (micros > 0)
                            tempoBpm = 60_000_000.0 / micros;
                    }
                    else if (metaType is 0x03 or 0x01 && metaLen > 0)
                    {
                        sequence.Title = Encoding.ASCII.GetString(metaData);
                    }

                    continue;
                }

                if (status is >= 0xF0 and <= 0xF7)
                {
                    // SysEx — skip
                    var sysexLen = ReadVarLen(data, ref offset);
                    offset += sysexLen;
                    continue;
                }

                var command = status & 0xF0;
                int dataBytes = command is 0xC0 or 0xD0 ? 1 : 2;
                if (offset + dataBytes > trackEnd)
                    break;
                var d1 = data[offset++];
                var d2 = dataBytes == 2 ? data[offset++] : (byte)0;

                if (command == 0x90 && d2 > 0)
                {
                    openNotes[(track, d1)] = (tick, d2);
                }
                else if (command is 0x80 || (command == 0x90 && d2 == 0))
                {
                    if (openNotes.Remove((track, d1), out var on))
                    {
                        var start = TicksToTime(on.Tick, tempoBpm, sequence.TicksPerQuarter);
                        var end = TicksToTime(tick, tempoBpm, sequence.TicksPerQuarter);
                        var dur = end > start ? end - start : TimeSpan.FromMilliseconds(50);
                        sequence.Add(new MidiNoteEvent(d1, on.Velocity, start, dur));
                    }
                }
            }

            offset = trackEnd;
        }

        sequence.TempoBpm = tempoBpm;
        _ = format; // format accepted for type 0/1
        return sequence;
    }

    static TimeSpan TicksToTime(int ticks, double tempoBpm, int ppq)
    {
        var beats = ticks / (double)ppq;
        return TimeSpan.FromMinutes(beats / tempoBpm);
    }

    static void WriteVarLen(Stream stream, int value)
    {
        value = Math.Max(0, value);
        var buffer = value & 0x7F;
        while ((value >>= 7) > 0)
        {
            buffer <<= 8;
            buffer |= (value & 0x7F) | 0x80;
        }

        while (true)
        {
            stream.WriteByte((byte)(buffer & 0xFF));
            if ((buffer & 0x80) == 0)
                break;
            buffer >>= 8;
        }
    }

    static int ReadVarLen(ReadOnlySpan<byte> data, ref int offset)
    {
        var value = 0;
        byte b;
        do
        {
            if (offset >= data.Length)
                throw new InvalidDataException("Truncated variable-length quantity.");
            b = data[offset++];
            value = (value << 7) | (b & 0x7F);
        } while ((b & 0x80) != 0);

        return value;
    }

    static void WriteBe16(Stream stream, ushort value)
    {
        Span<byte> b = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16BigEndian(b, value);
        stream.Write(b);
    }

    static void WriteBe32(Stream stream, int value)
    {
        Span<byte> b = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(b, value);
        stream.Write(b);
    }
}
