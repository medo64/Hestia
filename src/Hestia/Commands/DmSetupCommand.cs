namespace Hestia;

using System.Diagnostics;

internal static class DmSetupCommand {

    public static int InfoNoHeadingsUuid(string path, out string[] standardOutputLines, out string[] standardErrorLines) {
        var process = new Process {
            StartInfo = new ProcessStartInfo {
                FileName = "dmsetup",
                Arguments = $"info -c --noheadings -o uuid \"{path}\"",
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
