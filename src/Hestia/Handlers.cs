namespace Hestia;

using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

internal static class Handlers {

    public static async Task Default(HttpListenerResponse response) {
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
        sb.AppendLine("<head>");
        sb.AppendLine($"<title>{Environment.MachineName}</title>");
        sb.AppendLine("<link rel='stylesheet' href='/style.css'>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("<div>");
        sb.AppendLine("<h2>The following disks are still locked</h2>");
        sb.AppendLine("<table>");
        foreach (var disk in disks) {
            if (!disk.IsUnlocked) { sb.AppendLine($"<tr><td><code>{disk.DiskPath}</code></td></tr>"); }
        }
        sb.AppendLine("</table>");
        sb.AppendLine("</div>");
        sb.AppendLine("<div>");
        sb.AppendLine("<form method='POST' action='/unlock'>");
        sb.AppendLine("<input type='password' name='password' placeholder='Password'>");
        sb.AppendLine("<button type='submit'>Unlock</button>");
        sb.AppendLine("</form>");
        sb.AppendLine("</div>");
        sb.AppendLine("</body>");
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

        var sb = new StringBuilder();
        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine($"<title>{Environment.MachineName}</title>");
        sb.AppendLine("<link rel='stylesheet' href='/style.css'>");
        sb.AppendLine($"<meta http-equiv='refresh' content='3; URL={request.UrlReferrer}' />");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");

        // decrypt all locked disks
        var disks = new DiskById();
        foreach (var disk in disks) {
            if (!disk.IsUnlocked) {
                sb.Append("<div>");
                if (CryptSetupCommand.LuksOpen(disk.DiskPath, password, out var _, out var luksErrLines) == 0) {
                    sb.Append($"<h3>{disk.DiskPath} Decrypted</h3>");
                } else {
                    sb.Append($"<h3>{disk.DiskPath}</h3>");
                    sb.Append("<pre>");
                    foreach (var line in luksErrLines) { sb.AppendLine(WebUtility.HtmlEncode(line)); }
                    sb.Append("</pre>");
                }
                sb.Append("</div>");
            }
        }

        // check if all disks are now unlocked
        disks.Refresh();
        var allUnlocked = true;
        foreach (var disk in disks) {
            if (!disk.IsUnlocked) { allUnlocked = false; }
        }

        sb.AppendLine("<div>");
        if (allUnlocked) {
            sb.AppendLine("<h2>All disks unlocked</h2>");
        } else {
            sb.AppendLine("<h2>Some disks still locked</h2>");
        }
        sb.AppendLine("</div>");

        // restart docker
        sb.Append("<div>");
        if (SystemCtlCommand.Restart("docker", out var _, out var dockerErrLines) == 0) {
            sb.Append($"<h3>Docker Restarted</h3>");
        } else {
            sb.Append($"<h3>Docker Restart</h3>");
            sb.Append("<pre>");
            foreach (var line in dockerErrLines) { sb.AppendLine(WebUtility.HtmlEncode(line)); }
            sb.Append("</pre>");
        }
        sb.Append("</div>");

        sb.AppendLine("</body>");
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
        sb.AppendLine("<head>");
        sb.AppendLine($"<title>{Environment.MachineName}: Info</title>");
        sb.AppendLine("<link rel='stylesheet' href='/style.css'>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");

        {
            sb.AppendLine("<div>");
            sb.AppendLine("<h2>Encrypted Disks</h2>");
            sb.AppendLine("<table>");
            sb.AppendLine("<tr><th>Encrypted Disk ID</th><th>Decrypted Disk ID</th><th>LUKS UUID</th></tr>");
            var hadAny = false;
            var allUnlocked = true;
            foreach (var disk in disks) {
                if (!disk.IsUnlocked) {
                    sb.AppendLine($"<tr><td><code>{disk.DiskPath}</code></td><td></td><td><code>{disk.LuksUuid}</code></td></tr>");
                    allUnlocked = false;
                } else {
                    sb.AppendLine($"<tr><td><code>{disk.DiskPath}</code></td><td><code>{disk.MapperPath}</code></td><td><code>{disk.LuksUuid}</code></td></tr>");

                }
                hadAny = true;
            }
            if (!hadAny) { sb.AppendLine("<tr><td><em>No encrypted disks found</em></td></tr>"); }
            if (!allUnlocked) { sb.AppendLine("<tr><td><em><strong>Some disks are still encrypted</strong></em></td></tr>"); }
            sb.AppendLine("</table>");
            sb.AppendLine("</div>");
        }

        {
            sb.AppendLine("<div>");
            if (Docker.IsInstalled()) {
                sb.AppendLine("<h2>Docker Containers</h2>");
                sb.AppendLine("<table>");
                sb.AppendLine("<tr><th>Container Name</th></tr>");
                if (Docker.IsRunning()) {
                    var hadAny = false;
                    foreach (var containerName in Docker.GetRunningContainerNames()) {
                        sb.AppendLine($"<tr><td><code>{containerName}</code></td></tr>");
                        hadAny = true;
                    }
                    if (!hadAny) { sb.AppendLine("<tr><td><em>No running containers</em></td></tr>"); }
                } else {
                    sb.AppendLine("<tr><td><em><strong>Docker not running</strong></em></td></tr>");
                }
                if (Docker.IsEnabled()) { sb.AppendLine("<tr><td><em>Docker is enabled (shouldn't be)</em></td></tr>"); }
                sb.AppendLine("</table>");
            } else {
                sb.AppendLine("<h2>Docker Not Installed</h2>");
            }
            sb.AppendLine("</div>");
        }

        sb.AppendLine("</body>");
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
