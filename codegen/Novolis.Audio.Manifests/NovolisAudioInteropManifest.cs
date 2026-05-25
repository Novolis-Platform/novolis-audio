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
            SuppressGcTransitionByTemplate: new string[]
            {
                "void_void",
                "bool_void",
                "void_float",
                "void_nint",
                "bool_nint",
                "void_nint_float",
            },
            NeverSuppressGcTransition: new string[]
            {
                "na_LoadSound",
            },
            FacadeMethodImpl: "AggressiveInlining",
            UseDisableRuntimeMarshalling: true),
        Structs: Array.Empty<InteropStructSpec>(),
        Imports: new InteropImportSpec[]
        {
            new("na_Init", "bool_void", "Initialize the global audio engine."),
            new("na_Uninit", "void_void", "Shut down the engine and release device resources."),
            new("na_SetMasterVolume", "void_float", "Set master output volume in [0, 1]."),
            new("na_LoadSound", "nint_string_utf8", "Load a sound from a UTF-8 file path."),
            new("na_UnloadSound", "void_nint", "Release a sound loaded with na_LoadSound."),
            new("na_PlaySound", "bool_nint", "Start one-shot playback."),
            new("na_StopSound", "void_nint", "Stop playback for a sound."),
            new("na_IsSoundPlaying", "bool_nint", "Whether the sound is currently playing."),
            new("na_SetSoundVolume", "void_nint_float", "Set per-sound volume in [0, 1]."),
        });
}
