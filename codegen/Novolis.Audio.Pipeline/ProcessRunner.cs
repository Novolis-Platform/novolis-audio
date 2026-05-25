using System.Diagnostics;

namespace Novolis.Audio.Pipeline;

internal static class ProcessRunner
{
    public static async Task<int> RunAsync(
        PipelineContext context,
        string fileName,
        string arguments,
        string workingDirectory,
        CancellationToken cancellationToken)
    {
        await context.Log.WriteLineAsync($"$ {fileName} {arguments}");
        await context.Log.FlushAsync();

        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        using var process = Process.Start(psi) ?? throw new InvalidOperationException($"Failed to start {fileName}");

        var stdout = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderr = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        var outText = await stdout;
        var errText = await stderr;
        if (!string.IsNullOrEmpty(outText))
            await context.Log.WriteLineAsync(outText);
        if (!string.IsNullOrEmpty(errText))
            await context.Log.WriteLineAsync(errText);

        return process.ExitCode;
    }
}
