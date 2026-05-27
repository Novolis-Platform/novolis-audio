namespace Novolis.Audio.Pipeline;

internal static class PipelineProfiles
{
    public static IReadOnlyList<string> AllStepIds { get; } =
    [
        "step_01_vendor",
        "step_02_native",
        "step_03_verify_manifest",
        "step_03_voice_verify_models",
        "step_03_speech_verify_models",
        "step_04_codegen",
        "step_05_drift",
        "step_06_build",
    ];

    public static IReadOnlyList<string> Resolve(string profile) =>
        profile.ToLowerInvariant() switch
        {
            "all" or "maintainer" =>
            [
                "step_01_vendor",
                "step_02_native",
                "step_03_verify_manifest",
                "step_03_voice_verify_models",
                "step_03_speech_verify_models",
                "step_04_codegen",
                "step_05_drift",
            ],
            "generate" =>
            [
                "step_03_verify_manifest",
                "step_03_voice_verify_models",
                "step_03_speech_verify_models",
                "step_04_codegen",
            ],
            "agent-verify" =>
            [
                "step_03_verify_manifest",
                "step_03_voice_verify_models",
                "step_03_speech_verify_models",
                "step_04_codegen",
                "step_05_drift",
                "step_06_build",
            ],
            _ when AllStepIds.Contains(profile, StringComparer.Ordinal) => [profile],
            _ => throw new InvalidOperationException(
                $"Unknown profile '{profile}'. Use: all, maintainer, generate, agent-verify, or a step id."),
        };

    public static string? Explain(string stepId) =>
        stepId switch
        {
            "step_01_vendor" => "Fetch miniaudio.h into step_01_vendor/artifacts.",
            "step_02_native" => "CMake build novolis_audio shim; copy to step_02_native/artifacts.",
            "step_03_verify_manifest" => "Verify manifest imports exist in novolis_audio.h.",
            "step_03_voice_verify_models" => "Verify bundled voice model layout under models/.",
            "step_03_speech_verify_models" => "Verify bundled speech model layout under models/.",
            "step_04_codegen" => "Emit committed *.g.cs from manifests (bindings + voice catalog).",
            "step_05_drift" => "git diff on manifests and generated C#.",
            "step_06_build" => "Release build Bindings + Runtime + Abstractions.",
            _ => null,
        };
}
