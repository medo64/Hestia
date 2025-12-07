namespace Hestia;

using System.Diagnostics;
using System.IO;

internal static class CryptSetupCommand {

    public static int LuksDump(OutputStore? output, string diskPath, out string[] standardOutputLines, out string[] standardErrorLines) {
        var process = new Process {
            StartInfo = new ProcessStartInfo {
                FileName = "cryptsetup",
                Arguments = $"luksDump \"{diskPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        output?.Attach(process);
        process.WaitForExit();

        standardOutputLines = Helpers.SplitOutIntoLines(process.StandardOutput.ReadToEnd());
        standardErrorLines = Helpers.SplitOutIntoLines(process.StandardError.ReadToEnd());
        return process.ExitCode;
    }

    public static int LuksOpen(OutputStore? output, string diskPath, string password, out string[] standardOutputLines, out string[] standardErrorLines) {
        var luksName = Path.GetFileName(diskPath);
        var process = new Process {
            StartInfo = new ProcessStartInfo {
                FileName = "cryptsetup",
                Arguments = $"luksOpen \"{diskPath}\" \"{luksName}\"",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        var outCopy = output?.Attach(process);

        process.StandardInput.WriteLine(password);
        process.StandardInput.Flush();
        process.StandardInput.Close();

        process.WaitForExit();

        standardOutputLines = Helpers.SplitOutIntoLines((outCopy != null) ? outCopy.GetStdOut() : process.StandardOutput.ReadToEnd());
        standardErrorLines = Helpers.SplitOutIntoLines((outCopy != null) ? outCopy.GetStdErr() :process.StandardError.ReadToEnd());
        return process.ExitCode;
    }

}
