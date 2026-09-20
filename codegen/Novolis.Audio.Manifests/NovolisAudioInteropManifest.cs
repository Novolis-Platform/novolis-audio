using Novolis.CodeGen.Bindings;

namespace Novolis.Audio.Manifests;

public static partial class NovolisAudioBindingManifests
{
    public static InteropExportsFragment NovolisAudioInterop { get; } = new(
        Id: "novolis-audio",
        SchemaVersion: 2,
        Header: "codegen/vendor/novolis_audio/include/novolis_audio.h",
        Description: "Cross-platform novolis_audio shim (miniaudio engine) LibraryImport surface.",
        DllName: "novolis_audio",
        Policy: new(
            SuppressGcTransitionByFunction: [],
            NeverSuppressGcTransition: new string[]
            {
                "na_LoadSound",
            },
            FacadeMethodImpl: "AggressiveInlining",
            UseDisableRuntimeMarshalling: true),
        Structs: Array.Empty<InteropStructSpec>(),
        Imports: new InteropImportSpec[]
        {
            new("na_Init", AudioNativeSignatures.BoolVoid, "Initialize the global audio engine."),
            new("na_Uninit", AudioNativeSignatures.VoidVoid, "Shut down the engine and release device resources."),
            new("na_SetMasterVolume", AudioNativeSignatures.VoidFloat, "Set master output volume in [0, 1]."),
            new("na_LoadSound", AudioNativeSignatures.NativeIntUtf8, "Load a sound from a UTF-8 file path."),
            new("na_UnloadSound", AudioNativeSignatures.VoidHandle, "Release a sound loaded with na_LoadSound."),
            new("na_PlaySound", AudioNativeSignatures.BoolHandle, "Start one-shot playback."),
            new("na_StopSound", AudioNativeSignatures.VoidHandle, "Stop playback for a sound."),
            new("na_IsSoundPlaying", AudioNativeSignatures.BoolHandle, "Whether the sound is currently playing."),
            new("na_SetSoundVolume", AudioNativeSignatures.VoidHandleFloat, "Set per-sound volume in [0, 1]."),
        },
        Usings: ["Novolis.Audio.Interop"]);
}
