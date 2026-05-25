cmake_minimum_required(VERSION 3.24)
project(novolis_audio_platform C)

set(CMAKE_C_STANDARD 99)

get_filename_component(REPO_ROOT "${CMAKE_CURRENT_SOURCE_DIR}/../../.." ABSOLUTE)

if(NOT DEFINED MINIAUDIO_DIR)
  set(MINIAUDIO_DIR "${REPO_ROOT}/codegen/vendor/miniaudio" CACHE PATH "Directory containing miniaudio.h")
endif()

if(NOT EXISTS "${MINIAUDIO_DIR}/miniaudio.h")
  message(FATAL_ERROR "Missing ${MINIAUDIO_DIR}/miniaudio.h — run pipeline step_01_vendor or vendor miniaudio under codegen/vendor/miniaudio")
endif()

add_library(novolis_audio SHARED novolis_audio.c)
set_target_properties(novolis_audio PROPERTIES OUTPUT_NAME "novolis_audio")
target_compile_definitions(novolis_audio PRIVATE NOVOLIS_AUDIO_EXPORTS)
target_include_directories(novolis_audio PRIVATE
  "${MINIAUDIO_DIR}"
  "${REPO_ROOT}/codegen/vendor/novolis_audio/include")

if(WIN32)
  set(_OUT "${CMAKE_CURRENT_SOURCE_DIR}/out")
  set_target_properties(novolis_audio PROPERTIES
    RUNTIME_OUTPUT_DIRECTORY_DEBUG "${_OUT}"
    RUNTIME_OUTPUT_DIRECTORY_RELEASE "${_OUT}"
    RUNTIME_OUTPUT_DIRECTORY_RELWITHDEBINFO "${_OUT}"
    RUNTIME_OUTPUT_DIRECTORY_MINSIZEREL "${_OUT}")
else()
  set_target_properties(novolis_audio PROPERTIES LIBRARY_OUTPUT_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/out")
endif()
