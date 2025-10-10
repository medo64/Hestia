namespace Hestia;

using System.Diagnostics;

internal static class DockerCommand {

    public static int ListNames(out string[] standardOutputLines, out string[] standardErrorLines) {
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
        process.WaitForExit();

        standardOutputLines = Helpers.SplitOutIntoLines(process.StandardOutput.ReadToEnd());
        standardErrorLines = Helpers.SplitOutIntoLines(process.StandardError.ReadToEnd());
        return process.ExitCode;
    }

}
