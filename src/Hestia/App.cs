namespace Hestia;

using System;
using System.Net;
using System.Threading.Tasks;
using Medo.Configuration;

internal static class App {

    public static async Task Main(string[] args) {
        Config.FileName = "/etc/hestia.conf";

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
