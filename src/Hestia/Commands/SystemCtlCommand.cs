namespace Hestia;

using System.Diagnostics;

internal static class SystemCtlCommand {

    public static int IsActive(string serviceName, out string[] standardOutputLines, out string[] standardErrorLines) {
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
        process.WaitForExit();

        standardOutputLines = Helpers.SplitOutIntoLines(process.StandardOutput.ReadToEnd());
        standardErrorLines = Helpers.SplitOutIntoLines(process.StandardError.ReadToEnd());
        return process.ExitCode;
    }

    public static int IsEnabled(string serviceName, out string[] standardOutputLines, out string[] standardErrorLines) {
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
        process.WaitForExit();

        standardOutputLines = Helpers.SplitOutIntoLines(process.StandardOutput.ReadToEnd());
        standardErrorLines = Helpers.SplitOutIntoLines(process.StandardError.ReadToEnd());
        return process.ExitCode;
    }

    public static int Restart(string serviceName, out string[] standardOutputLines, out string[] standardErrorLines) {
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
        process.WaitForExit();

        standardOutputLines = Helpers.SplitOutIntoLines(process.StandardOutput.ReadToEnd());
        standardErrorLines = Helpers.SplitOutIntoLines(process.StandardError.ReadToEnd());
        return process.ExitCode;
    }

    public static int Status(string serviceName, out string[] standardOutputLines, out string[] standardErrorLines) {
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
        process.WaitForExit();

        standardOutputLines = Helpers.SplitOutIntoLines(process.StandardOutput.ReadToEnd());
        standardErrorLines = Helpers.SplitOutIntoLines(process.StandardError.ReadToEnd());
        return process.ExitCode;
    }

}
