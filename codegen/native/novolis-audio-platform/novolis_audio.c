#define MINIAUDIO_IMPLEMENTATION
#include "miniaudio.h"

#include "novolis_audio.h"

#include <stdlib.h>
#include <string.h>

typedef struct na_sound
{
    ma_sound sound;
    bool loaded;
} na_sound;

static ma_engine g_engine;
static bool g_initialized;

NA_API bool na_Init(void)
{
    if (g_initialized)
        return true;

    ma_result result = ma_engine_init(NULL, &g_engine);
    if (result != MA_SUCCESS)
        return false;

    g_initialized = true;
    return true;
}

NA_API void na_Uninit(void)
{
    if (!g_initialized)
        return;

    ma_engine_uninit(&g_engine);
    memset(&g_engine, 0, sizeof(g_engine));
    g_initialized = false;
}

NA_API void na_SetMasterVolume(float volume)
{
    if (!g_initialized)
        return;

    if (volume < 0.0f)
        volume = 0.0f;
    if (volume > 1.0f)
        volume = 1.0f;

    ma_engine_set_volume(&g_engine, volume);
}

NA_API na_sound_t na_LoadSound(const char* filePath)
{
    if (!g_initialized || filePath == NULL || filePath[0] == '\0')
        return NULL;

    na_sound* wrapper = (na_sound*)calloc(1, sizeof(na_sound));
    if (wrapper == NULL)
        return NULL;

    ma_result result = ma_sound_init_from_file(&g_engine, filePath, 0, NULL, NULL, &wrapper->sound);
    if (result != MA_SUCCESS)
    {
        free(wrapper);
        return NULL;
    }

    wrapper->loaded = true;
    return wrapper;
}

NA_API void na_UnloadSound(na_sound_t sound)
{
    if (sound == NULL)
        return;

    if (sound->loaded)
        ma_sound_uninit(&sound->sound);

    free(sound);
}

NA_API bool na_PlaySound(na_sound_t sound)
{
    if (sound == NULL || !sound->loaded)
        return false;

    ma_result result = ma_sound_start(&sound->sound);
    return result == MA_SUCCESS;
}

NA_API void na_StopSound(na_sound_t sound)
{
    if (sound == NULL || !sound->loaded)
        return;

    ma_sound_stop(&sound->sound);
}

NA_API bool na_IsSoundPlaying(na_sound_t sound)
{
    if (sound == NULL || !sound->loaded)
        return false;

    return ma_sound_is_playing(&sound->sound) == MA_TRUE;
}

NA_API void na_SetSoundVolume(na_sound_t sound, float volume)
{
    if (sound == NULL || !sound->loaded)
        return;

    if (volume < 0.0f)
        volume = 0.0f;
    if (volume > 1.0f)
        volume = 1.0f;

    ma_sound_set_volume(&sound->sound, volume);
}
