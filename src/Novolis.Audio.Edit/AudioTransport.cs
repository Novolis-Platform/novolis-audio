namespace Novolis.Audio.Edit;

/// <summary>Playhead / transport for arrangement scrubbing and preview.</summary>
public sealed class AudioTransport
{
    public TimeSpan Position { get; private set; }
    public bool IsPlaying { get; private set; }

    public event Action? Changed;

    public void Play()
    {
        if (IsPlaying)
            return;
        IsPlaying = true;
        Changed?.Invoke();
    }

    public void Pause()
    {
        if (!IsPlaying)
            return;
        IsPlaying = false;
        Changed?.Invoke();
    }

    public void Toggle()
    {
        if (IsPlaying)
            Pause();
        else
            Play();
    }

    public void Seek(TimeSpan position)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(position.Ticks);
        if (Position == position)
            return;
        Position = position;
        Changed?.Invoke();
    }

    public bool Tick(TimeSpan delta, TimeSpan projectDuration)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(delta.Ticks);
        ArgumentOutOfRangeException.ThrowIfNegative(projectDuration.Ticks);
        if (!IsPlaying || delta == TimeSpan.Zero)
            return false;

        var next = Position + delta;
        if (next >= projectDuration)
        {
            Position = projectDuration;
            IsPlaying = false;
            Changed?.Invoke();
            return true;
        }

        Position = next;
        Changed?.Invoke();
        return true;
    }
}
