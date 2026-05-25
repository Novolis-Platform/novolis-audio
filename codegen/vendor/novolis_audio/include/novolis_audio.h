#ifndef NOVOLIS_AUDIO_H
#define NOVOLIS_AUDIO_H

#include <stdbool.h>
#include <stdint.h>

#if defined(_WIN32) && defined(NOVOLIS_AUDIO_EXPORTS)
#define NA_API __declspec(dllexport)
#elif defined(_WIN32)
#define NA_API __declspec(dllimport)
#else
#define NA_API __attribute__((visibility("default")))
#endif

#ifdef __cplusplus
extern "C" {
#endif

/// Initialize the global miniaudio engine. Safe to call once; returns false on failure.
NA_API bool na_Init(void);

/// Shut down the engine and release all device resources.
NA_API void na_Uninit(void);

/// Master output volume in [0, 1].
NA_API void na_SetMasterVolume(float volume);

/// Opaque sound handle (heap-allocated wrapper around miniaudio sound).
typedef struct na_sound* na_sound_t;

/// Load a sound from a UTF-8 file path. Returns NULL on failure.
NA_API na_sound_t na_LoadSound(const char* filePath);

/// Release a sound loaded with na_LoadSound.
NA_API void na_UnloadSound(na_sound_t sound);

/// Start playback (one-shot). Returns false if the handle is invalid.
NA_API bool na_PlaySound(na_sound_t sound);

/// Stop playback for a sound.
NA_API void na_StopSound(na_sound_t sound);

/// Whether the sound is currently playing.
NA_API bool na_IsSoundPlaying(na_sound_t sound);

/// Per-sound volume in [0, 1].
NA_API void na_SetSoundVolume(na_sound_t sound, float volume);

#ifdef __cplusplus
}
#endif

#endif
