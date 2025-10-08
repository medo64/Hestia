namespace Hestia;

using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

internal static class Handlers {

    public static async Task Main(HttpListenerResponse response) {
        Log.Debug($"Processing root request");

        var disks = new DiskById();
        var allUnlocked = true;
        foreach (var disk in disks) {
            if (!disk.IsUnlocked) { allUnlocked = false; }
        }

        // if all disks are unlocked, just redirect to the target url
        if (allUnlocked) {
            Log.Info($"All disks unlocked, redirecting to {Settings.RedirectUrl}");
            response.StatusCode = (int)HttpStatusCode.TemporaryRedirect;
            response.RedirectLocation = Settings.RedirectUrl;
            return;
        }

        // otherwise show unlocking dialog
        var sb = new StringBuilder();
        sb.AppendLine("<html>");
        sb.AppendLine("  <head>");
        sb.AppendLine($"    <title>{Environment.MachineName}</title>");
        sb.AppendLine("    <link rel='stylesheet' href='/style.css'>");
        sb.AppendLine("  </head>");
        sb.AppendLine("  <body>");
        sb.AppendLine("    <div>");
        sb.AppendLine("      <h2>The following disks are still locked</h2>");
        sb.AppendLine("      <ul>");
        foreach (var disk in disks) {
            if (!disk.IsUnlocked) { sb.AppendLine($"        <li><code>{disk.DiskPath}</code></li>"); }
        }
        sb.AppendLine("      </ul>");
        sb.AppendLine("    </div>");
        sb.AppendLine("    <form method='POST' action='/unlock'>");
        sb.AppendLine("      <input type='password' name='password' placeholder='Password'>");
        sb.AppendLine("      <button type='submit'>Unlock</button>");
        sb.AppendLine("    </form>");
        sb.AppendLine("  </body>");
        sb.AppendLine("</html>");

        var buffer = Encoding.UTF8.GetBytes(sb.ToString());
        response.ContentLength64 = buffer.Length;
        response.ContentType = "text/html";
        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
    }

    public static async Task Unlock(HttpListenerRequest request, HttpListenerResponse response) {
        Log.Debug($"Processing unlock request");

        if (request.HttpMethod != "POST") {
            response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
            return;
        }

        string? password = null;
        using (var reader = new StreamReader(request.InputStream, request.ContentEncoding)) {
            var formData = await reader.ReadToEndAsync();
            var pairs = formData.Split('&');
            foreach (var pair in pairs) {
                var keyValue = pair.Split('=');
                if (keyValue.Length == 2 && keyValue[0] == "password") {
                    password = Uri.UnescapeDataString(keyValue[1]);
                    break;
                }
            }
        }
        if (password == null) {
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            return;
        }

        //TODO: Unlock

        var disks = new DiskById();
        var allUnlocked = true;
        foreach (var disk in disks) {
            if (!disk.IsUnlocked) { allUnlocked = false; }
        }

        var sb = new StringBuilder();
        sb.AppendLine("<html>");
        sb.AppendLine("  <head>");
        sb.AppendLine($"    <title>{Environment.MachineName}</title>");
        sb.AppendLine("    <link rel='stylesheet' href='/style.css'>");
        sb.AppendLine($"    <meta http-equiv='refresh' content='3; URL={request.UrlReferrer}' />");
        sb.AppendLine("  </head>");
        sb.AppendLine("  <body>");
        sb.AppendLine("    <div>");
        if (allUnlocked) {
            sb.AppendLine("      <h2>All disks unlocked</h2>");
        } else {
            sb.AppendLine("      <h2>Some disks still locked</h2>");
        }
        sb.AppendLine("    </div>");
        sb.AppendLine("  </body>");
        sb.AppendLine("</html>");

        var buffer = Encoding.UTF8.GetBytes(sb.ToString());
        response.ContentLength64 = buffer.Length;
        response.ContentType = "text/html";
        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
    }

    public static async Task Info(HttpListenerResponse response) {
        var disks = new DiskById();

        var sb = new StringBuilder();
        sb.AppendLine("<html>");
        sb.AppendLine("  <head>");
        sb.AppendLine($"    <title>{Environment.MachineName}: Info</title>");
        sb.AppendLine("    <link rel='stylesheet' href='/style.css'>");
        sb.AppendLine("  </head>");
        sb.AppendLine("<body>");

        {
            sb.AppendLine("    <h2>Encrypted Disks</h2>");
            sb.AppendLine("    <ul>");
            var hadAny = false;
            var allUnlocked = true;
            foreach (var disk in disks) {
                if (!disk.IsUnlocked) {
                    sb.AppendLine($"      <li><code>{disk.DiskPath}</code><br><code>{disk.LuksUuid}</code></li>");
                    allUnlocked = false;
                } else {
                    sb.AppendLine($"      <li><code>{disk.DiskPath}</code><br><code>{disk.MapperPath}</code><br><code>{disk.LuksUuid}</code></li>");

                }
                hadAny = true;
            }
            if (!hadAny) { sb.AppendLine("      <li style='list-style-type: none;'><em>No encrypted disks found</em></li>"); }
            if (!allUnlocked) { sb.AppendLine("      <li style='list-style-type: none;'><em>Some disks are still encrypted</em></li>"); }
            sb.AppendLine("    </ul>");
        }

        {
            if (Docker.IsInstalled()) {
                sb.AppendLine("    <h2>Docker Containers</h2>");
                sb.AppendLine("    <ul>");
                if (Docker.IsRunning()) {
                    var hadAny = false;
                    foreach (var containerName in Docker.GetRunningContainerNames()) {
                        sb.AppendLine($"      <li><code>{containerName}</code></li>");
                        hadAny = true;
                    }
                    if (!hadAny) { sb.AppendLine("      <li style='list-style-type: none;'><em>No running containers</em></li>"); }
                } else {
                    sb.AppendLine("      <li style='list-style-type: none;'><em>Docker not running</em></li>");
                }
                if (Docker.IsEnabled()) { sb.AppendLine("      <li style='list-style-type: none;'><em>Docker is enabled (shouldn't be)</em></li>"); }
                sb.AppendLine("    </ul>");
            } else {
                sb.AppendLine("    <h2>Docker Not Installed</h2>");
            }
        }

        sb.AppendLine("  </body>");
        sb.AppendLine("</html>");

        var buffer = Encoding.UTF8.GetBytes(sb.ToString());
        response.ContentLength64 = buffer.Length;
        response.ContentType = "text/html";
        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
    }

    public static async Task File(HttpListenerResponse response, string fileName) {
        var assembly = Assembly.GetExecutingAssembly();
        var resStream = assembly.GetManifestResourceStream("Hestia.Assets." + fileName);
        var buffer = new byte[(int)resStream!.Length];
        resStream.Read(buffer, 0, buffer.Length);

        response.ContentLength64 = buffer.Length;
        if (fileName.EndsWith(".css", StringComparison.OrdinalIgnoreCase)) {
            response.ContentType = "text/css";
        } else {
            response.ContentType = "application/octet-stream";
        }
        ;
        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
    }

}
