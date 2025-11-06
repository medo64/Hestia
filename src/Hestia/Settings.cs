namespace Hestia;

internal static class Settings {

#if DEBUG
    public static string Url => "http://*:8072/";
    public static string RedirectUrl => "https://medo64.com/";
#else
    public static string Url => "http://*:80/";
    public static string RedirectUrl => "";
#endif

}
