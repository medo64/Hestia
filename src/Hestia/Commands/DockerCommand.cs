namespace Hestia;

using System.Diagnostics;

internal static class DockerCommand {

    public static int ListNames(OutputStore? output, out string[] standardOutputLines, out string[] standardErrorLines) {
        var process = new Process {
            StartInfo = new ProcessStartInfo {
                FileName = "docker",
                Arguments = $"--format '{{.Names}}'",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        var outCopy = output?.Attach(process);
        process.WaitForExit();

        standardOutputLines = OutputStore.SplitStdOutIntoLines(process, outCopy);
        standardErrorLines = OutputStore.SplitStdErrIntoLines(process, outCopy);
        return process.ExitCode;
    }

}
