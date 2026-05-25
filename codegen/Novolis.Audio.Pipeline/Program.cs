using Novolis.Audio.CodeGen;

namespace Novolis.Audio.Pipeline;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        if (args is [] or ["-h"] or ["--help"])
        {
            PrintHelp();
            return args is [] ? 1 : 0;
        }

        var force = args.Contains("--force", StringComparer.Ordinal);
        var filtered = args.Where(a => !string.Equals(a, "--force", StringComparison.Ordinal)).ToArray();
        if (filtered.Length == 0)
        {
            PrintHelp();
            return 1;
        }

        var layout = AudioPipelineLayout.Find();
        var runner = new PipelineRunner(PipelineStepRegistry.CreateAll(), layout);

        try
        {
            return filtered[0].ToLowerInvariant() switch
            {
                "run" when filtered.Length >= 2 => await runner.RunProfileAsync(PipelineProfiles.Resolve(filtered[1]), force),
                "list" => ListSteps(),
                "explain" when filtered.Length >= 2 => Explain(filtered[1]),
                _ => Unknown(filtered[0]),
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }

    private static int ListSteps()
    {
        foreach (var step in PipelineStepRegistry.CreateAll())
            Console.WriteLine($"{step.Id,-28}  {step.Description}");
        Console.WriteLine();
        Console.WriteLine("Profiles: all, maintainer, generate, agent-verify");
        return 0;
    }

    private static int Explain(string stepId)
    {
        var text = PipelineProfiles.Explain(stepId);
        if (text is null)
        {
            Console.Error.WriteLine($"Unknown step: {stepId}");
            return 1;
        }

        Console.WriteLine(text);
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
            Novolis.Audio — linear maintainer pipeline

            Commands:
              run <profile|step_id> [--force]  — run a profile or single step
              list                             — list steps and profiles
              explain <step_id>                — describe a step

            Profiles:
              all / maintainer   step_01 .. step_05
              generate           step_03 + step_04
              agent-verify       step_03 .. step_06
            """);
    }
}
