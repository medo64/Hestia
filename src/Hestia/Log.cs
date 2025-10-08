namespace Hestia;

using System;

internal static class Log {

    public static void Trace(string message) {
        WriteLine("TRACE", message, "\x1b[34m", "\x1b[34m");
    }

    public static void Debug(string message) {
        WriteLine("DEBUG", message, "\x1b[34m", "\x1b[94m");
    }

    public static void Info(string message) {
        WriteLine("INFO", message, "\x1b[36m", "\x1b[96m");
    }

    public static void Warning(string message) {
        WriteLine("WARN", message, "\x1b[33m", "\x1b[93m");
    }

    public static void Error(string message) {
        WriteLine("ERROR", message, "\x1b[31m", "\x1b[91m");
    }

    public static void Fatal(string message) {
        WriteLine("FATAL", message, "\x1b[91m", "\x1b[91m");
    }

    private static void WriteLine(string level, string message, string ansiDim, string ansiBright) {
        var now = DateTime.Now;
        var ansiReset = "\x1b[0m";
        Console.WriteLine($"{ansiDim}{now.ToString("yyyy-MM-dd HH:mm:ss")} {level} {ansiBright}{message}{ansiReset}");
    }
}
