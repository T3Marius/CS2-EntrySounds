using static CounterStrikeSharp.API.Core.Listeners;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using CS2ScreenMenuAPI;
using T3MenuSharedApi;

namespace CS2EntrySounds;

public class CS2EntrySounds : BasePlugin, IPluginConfig<PluginConfig>
{
    public override string ModuleAuthor => "T3Marius";
    public override string ModuleName => "CS2-EntrySounds";
    public override string ModuleVersion => "1.4";

    public IT3MenuManager? MenuManager;
    public IT3MenuManager? GetMenuManager()
    {
        if (MenuManager == null)
            MenuManager = new PluginCapability<IT3MenuManager>("t3menu:manager").Get();

        return MenuManager;
    }

    public PluginConfig Config { get; set; } = new PluginConfig();
    public void OnConfigParsed(PluginConfig config)
    {
        Config = config;
    }
    public override void Load(bool hotReload)
    {
        RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
        foreach (var cmd in Config.Settings.MenuCommands)
        {
            AddCommand($"css_{cmd}", "Opens Menu", Command_Menu);
        }


        RegisterListener<OnServerPrecacheResources>((manifest) =>
        {
            foreach (var file in Config.Settings.SoundEventFiles)
            {
                manifest.AddResource(file);
            }
        });
    }
    public override void OnAllPluginsLoaded(bool hotReload)
    {
        Cookies.LoadClientPrefs();

        if (hotReload)
        {
            Cookies.ReloadClientprefs();
        }
    }
    public override void Unload(bool hotReload)
    {
        Cookies.UnloadClientprefis();
    }
    public HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if (player == null || player.IsBot || player.IsHLTV)
            return HookResult.Continue;

        foreach (var kvp in Config.EntrySounds)
        {
            var sound = kvp.Value;

            if (sound.SteamID == player.SteamID.ToString() || sound.Flags.Count > 0 && sound.Flags.Any(flag => AdminManager.PlayerHasPermissions(player, flag)))
            {
                foreach (var p in Utilities.GetPlayers())
                {
                    float volume = Config.Settings.DefaultVolume;
                    if (Cookies.playerVolumeCookies.TryGetValue(p, out string? volumeStr) && !string.IsNullOrEmpty(volumeStr))
                    {
                        float.TryParse(volumeStr, out volume);
                    }

                    if (volume > 0)
                    {
                        RecipientFilter filter = [p];
                        p.EmitSound(sound.Sound, filter, volume);
                    }

                    if (!string.IsNullOrEmpty(sound.JoinMessage))
                    {
                        PrintMessage(p, sound.MessageType, sound.JoinMessage);
                    }
                }
            }
        }

        return HookResult.Continue;
    }
    public void Command_Menu(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null)
            return;

        switch (Config.Settings.MenuType)
        {
            case "screen":
                ShowScreenMenu(player);
                break;

            case "t3":
                ShowT3Menu(player);
                break;
        }
    }
    public void ShowScreenMenu(CCSPlayerController player)
    {
        Menu menu = new Menu(player, this)
        {
            Title = Localizer.ForPlayer(player, "menu<title>"),
            ShowDisabledOptionNum = true
        };

        foreach (var volume in Config.Settings.VolumeOptions)
        {
            float volumeDecimal = volume / 100.0f;
            string volumeString = volumeDecimal.ToString();

            bool isSelected = Cookies.playerVolumeCookies.TryGetValue(player, out var currentVolume)
                              && currentVolume == volumeString;

            menu.AddItem(volume.ToString() + "%", (p, o) =>
            {
                if (Cookies.CLIENT_PREFS_API != null && Cookies.VolumeCookie != -1)
                {
                    Cookies.CLIENT_PREFS_API.SetPlayerCookie(p, Cookies.VolumeCookie, volumeString);
                    Cookies.playerVolumeCookies[p] = volumeDecimal.ToString();
                    p.PrintToChat(Localizer["prefix"] + Localizer["volume.selected", volume.ToString()]);
                }
            }, isSelected);
        }
        menu.Display();
    }
    public void ShowT3Menu(CCSPlayerController player)
    {
        IT3MenuManager? manager = GetMenuManager() ?? throw new Exception("T3MenuAPI not found");

        IT3Menu? menu = manager.CreateMenu(Localizer.ForPlayer(player, "menu<title>"));
        List<object> volumeValues = Config.Settings.VolumeOptions.Cast<object>().ToList();

        object defaultVolume = volumeValues.FirstOrDefault() ?? Config.Settings.DefaultVolume;

        if (Cookies.VolumeCookie != -1 && Cookies.playerVolumeCookies.TryGetValue(player, out string? savedVolume))
        {
            if (float.TryParse(savedVolume, out float savedVolumeValue))
            {
                int savedPercentage = (int)(savedVolumeValue * 100);

                int closestValue = Config.Settings.VolumeOptions.OrderBy(v => Math.Abs(v - savedPercentage)).FirstOrDefault();

                defaultVolume = closestValue;
            }
        }

        menu.AddSliderOption(Localizer.ForPlayer(player, "slider<volume>"), volumeValues, defaultVolume, 3, (p, o, index) =>
        {
            if (o is IT3Option sliderOption && sliderOption.DefaultValue != null)
            {
                int volumePercentage = Convert.ToInt32(sliderOption.DefaultValue);
                float volumeDecimal = volumePercentage / 100.0f;

                if (Cookies.CLIENT_PREFS_API != null && Cookies.VolumeCookie != -1)
                {
                    Cookies.CLIENT_PREFS_API.SetPlayerCookie(p, Cookies.VolumeCookie, volumeDecimal.ToString());
                    Cookies.playerVolumeCookies[p] = volumeDecimal.ToString();

                    p.PrintToChat(Localizer["prefix"] + Localizer["volume.selected", volumePercentage]);
                }
            }
        });
        manager.OpenMainMenu(player, menu);
    }
    public void PrintMessage(CCSPlayerController player, string type, string message)
    {

        switch (type)
        {
            case "chat":
                string coloredMessage = StringExtensions.ReplaceColorTags(message);
                player.PrintToChat(coloredMessage.Replace("{playername}", player.PlayerName));
                break;

            case "center":
                player.PrintToCenter(message.Replace("{playername}", player.PlayerName));
                break;

            case "alert":
                player.PrintToCenterAlert(message.Replace("{playername}", player.PlayerName));
                break;
        }
    }

}