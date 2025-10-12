namespace Hestia;

using System.Collections.Generic;

internal static class Zfs {

    public static string[] GetPoolsForImport() {
        if (ZPoolCommand.Import(out var stdOut, out var _) == 0) {
            var pools = new List<string>();
            foreach (var line in stdOut) {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith("pool: ")) {
                    var poolName = trimmedLine["pool: ".Length..].Trim();
                    pools.Add(poolName);
                }
            }
            return [.. pools];
        }
        return [];
    }

}
