using Novolis.CodeGen.Bindings;

namespace Novolis.Audio.Manifests;

public static partial class NovolisAudioBindingManifests
{
    public static FacadeTypesFragment Facades { get; } = new(
        Id: "facades",
        Types: new FacadeTypeSpec[]
        {
            new(
                Name: "AudioDevice",
                Namespace: "Novolis.Audio.Runtime",
                Folder: "Device",
                TypeSummary: "Global audio device lifecycle (miniaudio engine).",
                Usings: new[] { "Novolis.Audio.Interop" },
                Methods: new FacadeMethodSpec[]
                {
                    new("Init", "bool Init()", "NovolisAudioNative.na_Init()", "Initialize the global audio engine."),
                    new("Uninit", "void Uninit()", "NovolisAudioNative.na_Uninit()", "Shut down the engine."),
                    new("SetMasterVolume", "void SetMasterVolume(float volume)", "NovolisAudioNative.na_SetMasterVolume(volume)", "Set master output volume."),
                }),
            new(
                Name: "Sound",
                Namespace: "Novolis.Audio.Runtime",
                Folder: "Sound",
                TypeSummary: "Load and play decoded audio files.",
                Usings: new[] { "Novolis.Audio.Interop" },
                Methods: new FacadeMethodSpec[]
                {
                    new("Load", "nint Load(string path)", "NovolisAudioNative.na_LoadSound(path)", "Load a sound from disk."),
                    new("Unload", "void Unload(nint handle)", "NovolisAudioNative.na_UnloadSound(handle)", "Release a loaded sound."),
                    new("Play", "bool Play(nint handle)", "NovolisAudioNative.na_PlaySound(handle)", "Start playback."),
                    new("Stop", "void Stop(nint handle)", "NovolisAudioNative.na_StopSound(handle)", "Stop playback."),
                    new("IsPlaying", "bool IsPlaying(nint handle)", "NovolisAudioNative.na_IsSoundPlaying(handle)", "Whether the sound is playing."),
                    new("SetVolume", "void SetVolume(nint handle, float volume)", "NovolisAudioNative.na_SetSoundVolume(handle, volume)", "Set per-sound volume."),
                }),
        });
}
