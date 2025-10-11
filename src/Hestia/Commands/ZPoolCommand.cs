namespace Hestia;

using System.Diagnostics;

internal static class ZPoolCommand {

    public static int Import(out string[] standardOutputLines, out string[] standardErrorLines) {
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
        process.WaitForExit();

        standardOutputLines = Helpers.SplitOutIntoLines(process.StandardOutput.ReadToEnd());
        standardErrorLines = Helpers.SplitOutIntoLines(process.StandardError.ReadToEnd());
        return process.ExitCode;
    }

    public static int Import(string poolName, out string[] standardOutputLines, out string[] standardErrorLines) {
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
        process.WaitForExit();

        standardOutputLines = Helpers.SplitOutIntoLines(process.StandardOutput.ReadToEnd());
        standardErrorLines = Helpers.SplitOutIntoLines(process.StandardError.ReadToEnd());
        return process.ExitCode;
    }

}
