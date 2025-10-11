namespace Hestia;

using System.Collections.Generic;

internal static class Zfs {

    public static string[] GetPoolsForImport() {
        if (ZPoolCommand.Import(out var stdOut, out var _) == 0) {
            var pools = new List<string>();
            foreach (var line in stdOut) {
                line.Trim();
                if (line.StartsWith("pool: ")) {
                    var poolName = line["pool: ".Length..].Trim();
                    pools.Add(poolName);
                }
            }
            return [.. pools];
        }
        return [];
    }

}
