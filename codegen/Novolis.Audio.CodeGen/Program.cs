namespace Novolis.Audio.CodeGen;

internal static class Program
{
    public static int Main(string[] args)
    {
        if (args is [] or ["-h"] or ["--help"])
        {
            PrintHelp();
            return args is [] ? 1 : 0;
        }

        var repoRoot = PipelinePaths.FindRepoRoot();
        var cmd = args[0].ToLowerInvariant();

        try
        {
            return cmd switch
            {
                "generate" => Generate(repoRoot),
                "verify" => AudioManifestVerifier.Verify(repoRoot),
                _ => Unknown(cmd),
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }

    private static int Generate(string repoRoot)
    {
        if (VoiceModelVerifier.Verify(repoRoot) != 0)
            return 1;

        if (SpeechModelVerifier.Verify(repoRoot) != 0)
            return 1;

        var pipeline = new AudioCodegenPipeline(repoRoot);
        pipeline.GenerateAllVoiceAndBindings();
        return 0;
    }

    private static int Unknown(string cmd)
    {
        Console.Error.WriteLine($"Unknown command: {cmd}");
        PrintHelp();
        return 1;
    }

    private static void PrintHelp()
    {
        Console.WriteLine("""
            Novolis.Audio — miniaudio / novolis_audio Roslyn codegen

            Commands:
              generate  — emit interop + façades + voice model catalog
              verify    — fail if manifest symbols missing from novolis_audio.h

            Maintainer pipeline: dotnet run --project codegen/Novolis.Audio.Pipeline -- run maintainer
            """);
    }
}
