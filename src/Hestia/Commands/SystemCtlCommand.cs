namespace Hestia;

using System.Diagnostics;

internal static class SystemCtlCommand {

    public static int IsActive(OutputStore? output, string serviceName, out string[] standardOutputLines, out string[] standardErrorLines) {
        var process = new Process {
            StartInfo = new ProcessStartInfo {
                FileName = "systemctl",
                Arguments = $"is-active {serviceName}",
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

    public static int IsEnabled(OutputStore? output, string serviceName, out string[] standardOutputLines, out string[] standardErrorLines) {
        var process = new Process {
            StartInfo = new ProcessStartInfo {
                FileName = "systemctl",
                Arguments = $"is-enabled {serviceName}",
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

    public static int Restart(OutputStore? output, string serviceName, out string[] standardOutputLines, out string[] standardErrorLines) {
        var process = new Process {
            StartInfo = new ProcessStartInfo {
                FileName = "systemctl",
                Arguments = $"restart {serviceName} --no-pager --lines=0",
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

    public static int Status(OutputStore? output, string serviceName, out string[] standardOutputLines, out string[] standardErrorLines) {
        var process = new Process {
            StartInfo = new ProcessStartInfo {
                FileName = "systemctl",
                Arguments = $"status {serviceName} --no-pager --lines=0",
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
