using CounterStrikeSharp.API.Core;

namespace CS2EntrySounds;

public class PluginConfig : BasePluginConfig
{
    public Settings_Config Settings { get; set; } = new Settings_Config();
    public Dictionary<string, Entry_Sound> EntrySounds { get; set; } = new Dictionary<string, Entry_Sound>();
}
public class Settings_Config
{
    public List<string> SoundEventFiles { get; set; } = [];
    public string MenuType { get; set; } = "t3";
    public List<string> MenuCommands { get; set; } = ["es", "entrysounds"];
    public float DefaultVolume { get; set; } = 20;
    public List<int> VolumeOptions { get; set; } = [0, 20, 40, 60, 80, 100];
}
public class Entry_Sound
{
    public string Sound { get; set; } = string.Empty;
    public List<string> Flags { get; set; } = new List<string>();
    public string SteamID { get; set; } = string.Empty;
    public string JoinMessage { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
}