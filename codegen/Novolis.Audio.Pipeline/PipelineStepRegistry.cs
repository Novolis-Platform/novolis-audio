using Novolis.Audio.Pipeline.Steps;

namespace Novolis.Audio.Pipeline;

internal static class PipelineStepRegistry
{
    public static IReadOnlyList<IPipelineStep> CreateAll() =>
    [
        new VendorStep(),
        new NativeStep(),
        new VerifyManifestStep(),
        new CodegenStep(),
        new DriftStep(),
        new BuildStep(),
    ];
}
