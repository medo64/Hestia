namespace Hestia;

using System.Diagnostics;

internal static class TmpUsbCommand {

    public static int Mount(OutputStore? output) {
        var process = new Process {
            StartInfo = new ProcessStartInfo {
                FileName = "tmpusb",
                Arguments = "-m",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        output?.Attach(process);
        process.WaitForExit();

        return process.ExitCode;
    }

    public static int Unmount(OutputStore? output) {
        var process = new Process {
            StartInfo = new ProcessStartInfo {
                FileName = "tmpusb",
                Arguments = "-u",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        output?.Attach(process);
        process.WaitForExit();

        return process.ExitCode;
    }

}
