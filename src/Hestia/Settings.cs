namespace Hestia;

using Medo.Configuration;

internal static class Settings {

    public static string ListenPrefix {
        get {
#if DEBUG
            return Config.Read("Listen", "http://*:8072/");
#else
            return Config.Read("Listen", "http://*:80/");
#endif
        }
    }

    public static string RedirectUrl {
        get {
#if DEBUG
            return Config.Read("Redirect", "https://medo64.com/");
#else
            return Config.Read("Redirect", "");
#endif
        }
    }

    public static string TmpUsbFile {
        get {
            return Config.Read("TmpUsbFile", "");
        }
    }

}
