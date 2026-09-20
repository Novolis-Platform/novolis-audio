using Novolis.CodeGen.Bindings;

namespace Novolis.Audio.Manifests;

/// <summary>Typed C ABI signatures exported by the novolis_audio native shim.</summary>
public static class AudioNativeSignatures
{
    /// <summary>Boolean function with no parameters.</summary>
    public static NativeSignature BoolVoid { get; } = NativeSignature.Create(NativeType.Boolean);

    /// <summary>Void function with no parameters.</summary>
    public static NativeSignature VoidVoid { get; } = NativeSignature.Create(NativeType.Void);

    /// <summary>Void function with a floating-point volume.</summary>
    public static NativeSignature VoidFloat { get; } = NativeSignature.Create(
        NativeType.Void,
        new NativeParameter("volume", NativeType.Float));

    /// <summary>Native handle loader with a UTF-8 file path.</summary>
    public static NativeSignature NativeIntUtf8 { get; } = NativeSignature.Create(
        NativeType.NativeInt,
        new NativeParameter("filePath", NativeType.Utf8String));

    /// <summary>Void function with an opaque sound handle.</summary>
    public static NativeSignature VoidHandle { get; } = NativeSignature.Create(
        NativeType.Void,
        new NativeParameter("handle", NativeType.NativeInt));

    /// <summary>Boolean function with an opaque sound handle.</summary>
    public static NativeSignature BoolHandle { get; } = NativeSignature.Create(
        NativeType.Boolean,
        new NativeParameter("handle", NativeType.NativeInt));

    /// <summary>Void function with an opaque sound handle and volume.</summary>
    public static NativeSignature VoidHandleFloat { get; } = NativeSignature.Create(
        NativeType.Void,
        new NativeParameter("handle", NativeType.NativeInt),
        new NativeParameter("volume", NativeType.Float));
}
