namespace Hestia;

using System.Collections;
using System.Collections.Generic;
using System.IO;

internal class DiskById : IEnumerable<DiskInfo> {

    public DiskById() {
        Refresh();

        //TODO: Remove - added for testing
        Disks.Add(new DiskInfo("/dev/disk/by-id/test", "", "TEST"));
    }

    public void Refresh() {
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

        Disks.Clear();
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
            Disks.Add(info);
            Log.Debug($"{info.DiskPath} -> {info.LuksUuid} -> {info.MapperPath}");
            luksUuidTracker.Add(luksUuid, diskPath);
        }
    }

    private readonly IList<DiskInfo> Disks = [];

    private string GetLuksUuidViaLuksDump(string path) {
        if (CryptSetupCommand.LuksDump(path, out var outLines, out var _) == 0) {
            foreach (var line in outLines) {
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
        if (DmSetupCommand.InfoNoHeadingsUuid(path, out var outLines, out var _) == 0) {
            if (outLines.Length == 1) {
                if (outLines[0].StartsWith("CRYPT-LUKS2", System.StringComparison.Ordinal)) {
                    var parts = outLines[0].Split('-');
                    if (parts.Length > 2) { return parts[2].Trim(); }
                }
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
