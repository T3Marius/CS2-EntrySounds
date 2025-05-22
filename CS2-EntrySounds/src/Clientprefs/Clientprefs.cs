using Clientprefs;
using Clientprefs.API;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;

namespace CS2EntrySounds;

public static class Cookies
{
    public static int VolumeCookie = -1;
    public static Dictionary<CCSPlayerController, string> playerVolumeCookies { get; set; } = new();

    public static readonly PluginCapability<IClientprefsApi> cookieCapabilty = new("Clientprefs");
    public static IClientprefsApi? CLIENT_PREFS_API;

    public static void LoadClientPrefs()
    {
        try
        {
            CLIENT_PREFS_API = cookieCapabilty.Get() ?? throw new Exception("Clientprefs api not found");
            CLIENT_PREFS_API.OnDatabaseLoaded += OnClientprefDatabaseReady;
            CLIENT_PREFS_API.OnPlayerCookiesCached += OnPlayerCookiesCached;
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Failed to load CleintprefsApi! | {ex.Message}");
        }
    }
    public static void UnloadClientprefis()
    {
        if (CLIENT_PREFS_API == null)
            return;

        CLIENT_PREFS_API.OnDatabaseLoaded -= OnClientprefDatabaseReady;
        CLIENT_PREFS_API.OnPlayerCookiesCached -= OnPlayerCookiesCached;
    }
    public static void ReloadClientprefs()
    {
        if (CLIENT_PREFS_API == null || VolumeCookie == -1) return;

        foreach (CCSPlayerController player in Utilities.GetPlayers().Where(p => !p.IsBot))
        {
            if (!CLIENT_PREFS_API.ArePlayerCookiesCached(player))
                continue;

            string volValue = CLIENT_PREFS_API.GetPlayerCookie(player, VolumeCookie);
            if (!string.IsNullOrEmpty(volValue))
                playerVolumeCookies[player] = volValue;
        }
    }
    public static void OnClientprefDatabaseReady()
    {
        if (CLIENT_PREFS_API == null) return;

        VolumeCookie = CLIENT_PREFS_API.RegPlayerCookie("Volume", "Player volume preference", CookieAccess.CookieAccess_Public);
        if (VolumeCookie == -1)
        {
            System.Console.WriteLine("Failed to register Volume cookie");
        }
    }

    public static void OnPlayerCookiesCached(CCSPlayerController player)
    {
        if (CLIENT_PREFS_API == null || VolumeCookie == -1) return;

        string volValue = CLIENT_PREFS_API.GetPlayerCookie(player, VolumeCookie);
        if (!string.IsNullOrEmpty(volValue))
            playerVolumeCookies[player] = volValue;
    }

}