namespace Hestia;

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

internal class DiskById : IEnumerable<DiskInfo> {

    public DiskById() {
        var idPaths = new List<string>();
        foreach (var file in Directory.EnumerateFiles("/dev/disk/by-id/")) {
            var path = Path.GetFullPath(file);
            var name = Path.GetFileName(file);
            if (name.StartsWith("dm-name-")) { continue; }
            if (name.StartsWith("dm-uuid-")) { continue; }
            if (name.StartsWith("nvme-eui.")) { continue; }
            Log.Trace($"{path}");
            idPaths.Add(path);
        }
        idPaths.Sort();

        var mapperUuidAndPath = new Dictionary<string, string>();
        foreach (var file in Directory.EnumerateFiles("/dev/mapper/")) {
            var path = Path.GetFullPath(file);
            var name = Path.GetFileName(file);
            if (name.StartsWith("control")) { continue; }
            var luksUuid = GetLuksUuidViaDmSetup(path);
            if (string.IsNullOrWhiteSpace(luksUuid)) { continue; }
            Log.Trace($"{path} -> {luksUuid}");
            mapperUuidAndPath.Add(luksUuid, path);
        }

        var luksUuidTracker = new Dictionary<string, string>();

        var disks = new List<DiskInfo>();
        foreach (var diskPath in idPaths) {
            var luksUuid = GetLuksUuidViaLuksDump(diskPath);
            if (string.IsNullOrWhiteSpace(luksUuid)) { continue; }
            if (luksUuidTracker.ContainsKey(luksUuid)) { continue; }
            DiskInfo info;
            if (mapperUuidAndPath.TryGetValue(luksUuid, out var mapperPath)) {
                info = new DiskInfo(diskPath, mapperPath, luksUuid);
            } else {
                info = new DiskInfo(diskPath, "", luksUuid);
            }
            disks.Add(info);
            Log.Debug($"{info.DiskPath} -> {info.LuksUuid} -> {info.MapperPath}");
            luksUuidTracker.Add(luksUuid, diskPath);
        }

        //TODO: Remove - added for testing
        disks.Add(new DiskInfo("/dev/disk/by-id/test", "", "TEST"));

        Disks = disks;
    }

    private IList<DiskInfo> Disks;

    private string GetLuksUuidViaLuksDump(string path) {
        var process = new Process {
            StartInfo = new ProcessStartInfo {
                FileName = "cryptsetup",
                Arguments = $"luksDump \"{path}\"",
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
            foreach (var line in output.Split('\n')) {
                if (line.Trim().StartsWith("UUID:")) {
                    var parts = line.Split(':', 2);
                    if (parts.Length == 2) {
                        return parts[1].Trim().Replace("-", "");
                    }
                }
            }
        }
        return "";
    }

    private string GetLuksUuidViaDmSetup(string path) {
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
        if (process.ExitCode == 0) {
            var output = process.StandardOutput.ReadToEnd();
            var parts = output.Split('-');
            if (parts.Length > 2) {
                return parts[2].Trim();
            }
        }
        return "";
    }

    #region IEnumerable<DiskInfo>

    public IEnumerator<DiskInfo> GetEnumerator() {
        return Disks.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    #endregion IEnumerable<DiskInfo>

}
