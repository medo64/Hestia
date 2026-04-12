namespace Hestia;

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Medo;

internal static class App {

    public static async Task Main(string[] args) {
        Config.Initialize("/etc/hestia.conf");

        if (!string.IsNullOrEmpty(Settings.TmpUsbFile)) {
            Log.Info("Unlocking via TmpUsb");
            bool needsTmpUsbUnmount = false;

            if (Directory.Exists("/tmpusb") && Directory.GetFiles("/tmpusb").Length > 0) {
                Log.Debug("TmpUsb already mounted");
            } else {
                Log.Debug("Mounting TmpUsb");
                var exitCode = TmpUsbCommand.Mount(output: null);
                if (exitCode != 0) {
                    needsTmpUsbUnmount = true;
                    Log.Warning($"Error mounting TmpUsb (exit:{exitCode})");
                }
            }

            if (File.Exists($"/tmpusb/{Settings.TmpUsbFile}")) {
                var password = File.ReadAllText($"/tmpusb/{Settings.TmpUsbFile}").Trim();

                var anyUnlocked = false;
                var disks = new DiskById();
                foreach (var disk in disks) {
                    Log.Debug($"Decrypting disk {disk.DiskPath}");
                    if (!disk.IsUnlocked) {
                        if (CryptSetupCommand.LuksOpen(output: null, disk.DiskPath, password, out var _, out var _) == 0) {
                            anyUnlocked = true;
                            Log.Info($"Decrypted disk {disk.DiskPath}");
                        } else {
                            Log.Error($"Cannot decrypt disk {disk.DiskPath}");
                        }
                    }
                }

                if (anyUnlocked) {
                    // import zfs pools
                    var poolsToImport = Zfs.GetPoolsForImport(output: null);
                    if (poolsToImport.Length > 0) {
                        var swImport = Stopwatch.StartNew();
                        Parallel.ForEach(poolsToImport, poolName => {
                            if (ZPoolCommand.Import(output: null, poolName, out var _, out var _) == 0) {
                                Log.Info($"Imported ZFS pool {poolName}");
                            } else {
                                Log.Error($"Cannot import ZFS pool {poolName}");
                            }
                        });
                        Log.Debug($"ZFS import took {swImport.Elapsed.TotalMilliseconds:#,##0.0} ms");
                    }

                    // restart docker
                    var swDocker = Stopwatch.StartNew();
                    if (SystemCtlCommand.Restart(output: null, "docker", out var _, out var dockerErrLines) == 0) {
                        Log.Info($"Docker restarted");
                    } else {
                        Log.Error($"Docker restart failed");
                    }
                    Log.Debug($"Docker restart took {swDocker.Elapsed.TotalMilliseconds:#,##0.0} ms");
                } else {
                    Log.Warning("No disks unlocked");
                }

            } else {
                Log.Warning($"TmpUsb file not found: {Settings.TmpUsbFile}");
            }

            if (needsTmpUsbUnmount) {
                var exitCode = TmpUsbCommand.Mount(output: null);
                if (exitCode != 0) {
                    needsTmpUsbUnmount = true;
                    Log.Warning($"Error unmounting TmpUsb (exit:{exitCode})");
                }
            }
        }

        var listener = new HttpListener();
        listener.Prefixes.Add(Settings.ListenPrefix);
        listener.Start();

        Log.Info($"Web server started on {Settings.ListenPrefix}");

        while (true) {
            var context = await listener.GetContextAsync();
            Log.Trace($"Received request for {context.Request.Url?.AbsolutePath}");

            if ("/".Equals(context.Request.Url?.AbsolutePath, StringComparison.OrdinalIgnoreCase)) {
                await Handlers.Default(context.Response);
            } else if ("/info".Equals(context.Request.Url?.AbsolutePath, StringComparison.OrdinalIgnoreCase)) {
                await Handlers.Info(context.Response);
            } else if ("/output".Equals(context.Request.Url?.AbsolutePath, StringComparison.OrdinalIgnoreCase)) {
                await Handlers.Output(context.Request, context.Response);
            } else if ("/unlock".Equals(context.Request.Url?.AbsolutePath, StringComparison.OrdinalIgnoreCase)) {
                await Handlers.Unlock(context.Request, context.Response);
            } else if ("/style.css".Equals(context.Request.Url?.AbsolutePath, StringComparison.OrdinalIgnoreCase)) {
                await Handlers.File(context.Response, "style.css");
            } else {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }

            context.Response.OutputStream.Close();
        }
    }
}
