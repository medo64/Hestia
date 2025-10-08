namespace Hestia;

using System;
using System.Diagnostics;

internal static class Docker {

    public static bool IsInstalled() {
        var process = new Process {
            StartInfo = new ProcessStartInfo {
                FileName = "systemctl",
                Arguments = "status docker --no-pager --lines=0",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        process.WaitForExit();
        return process.ExitCode is 0 or 3;
    }

    public static bool IsEnabled() {
        var process = new Process {
            StartInfo = new ProcessStartInfo {
                FileName = "systemctl",
                Arguments = "is-enabled docker",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        process.WaitForExit();
        return (process.ExitCode == 0);
    }

    public static bool IsRunning() {
        var process = new Process {
            StartInfo = new ProcessStartInfo {
                FileName = "systemctl",
                Arguments = "is-active docker",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        process.WaitForExit();
        return (process.ExitCode == 0);
    }

    public static string[] GetRunningContainerNames() {
        var process = new Process {
            StartInfo = new ProcessStartInfo {
                FileName = "docker",
                Arguments = "--format '{{.Names}}'",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        process.WaitForExit();
        if (process.ExitCode == 0) {
            var output = process.StandardOutput.ReadToEnd();
            return output.Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }
        return [];
    }

}
