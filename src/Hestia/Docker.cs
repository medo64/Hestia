namespace Hestia;

internal static class Docker {

    public static bool IsInstalled() {
        return SystemCtlCommand.Status("docker", out _, out _) is 0 or 3;
    }

    public static bool IsEnabled() {
        return SystemCtlCommand.IsEnabled("docker", out _, out _) is 0;
    }

    public static bool IsRunning() {
        return SystemCtlCommand.IsActive("docker", out _, out _) is 0;
    }

    public static string[] GetRunningContainerNames() {
        if (DockerCommand.ListNames(out var stdOut, out var _) == 0) {
            return stdOut;
        }
        return [];
    }

}
