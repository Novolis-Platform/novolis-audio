using Novolis.CodeGen.Bindings;

namespace Novolis.Audio.CodeGen;

public sealed class AudioCodegenContext : BindingEmitContext
{
    public required AudioCodegenPhase Phase { get; init; }

    public string? FacadeTypeName { get; init; }

    public string? FacadeMethodImpl { get; init; }
}

public enum AudioCodegenPhase
{
    Interop,
    Facade,
}
