namespace Hestia;

using System.Diagnostics;

internal static class DmSetupCommand {

    public static int InfoNoHeadingsUuid(OutputStore? output, string mapperPath, out string[] standardOutputLines, out string[] standardErrorLines) {
        var process = new Process {
            StartInfo = new ProcessStartInfo {
                FileName = "dmsetup",
                Arguments = $"info -c --noheadings -o uuid \"{mapperPath}\"",
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
