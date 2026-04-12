namespace Hestia;

using System.Diagnostics;

internal static class ZPoolCommand {

    public static int Import(OutputStore? output, out string[] standardOutputLines, out string[] standardErrorLines) {
        var process = new Process {
            StartInfo = new ProcessStartInfo {
                FileName = "zpool",
                Arguments = "import",
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

    public static int Import(OutputStore? output, string poolName, out string[] standardOutputLines, out string[] standardErrorLines) {
        var process = new Process {
            StartInfo = new ProcessStartInfo {
                FileName = "zpool",
                Arguments = $"import {poolName}",
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
