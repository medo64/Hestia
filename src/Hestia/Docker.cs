namespace Hestia;

internal static class Docker {

    public static bool IsInstalled(OutputStore output) {
        return SystemCtlCommand.Status(output, "docker", out _, out _) is 0 or 3;
    }

    public static bool IsEnabled(OutputStore output) {
        return SystemCtlCommand.IsEnabled(output, "docker", out _, out _) is 0;
    }

    public static bool IsRunning(OutputStore output) {
        return SystemCtlCommand.IsActive(output, "docker", out _, out _) is 0;
    }

    public static string[] GetRunningContainerNames(OutputStore output) {
        if (DockerCommand.ListNames(output, out var stdOut, out var _) == 0) {
            return stdOut;
        }
        return [];
    }

}
