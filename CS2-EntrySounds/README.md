# CS2-EntrySounds
- It's a simple plugin that plays a sound and sends a message for player.
- You can set sounds/messages to flags and steamid's.

# Requirementes
- [ [T3MenuAPI](https://github.com/T3Marius/T3Menu-API) ]
- [ [ScreenMenu](https://github.com/T3Marius/CS2ScreenMenuAPI) ]

# Config example. You can use .json and .toml . To use .toml just rename your config from .json to .toml and vice-versa.

```toml
[Settings]
SoundEventFiles = []
MenuType = "t3"  # t3, screen
MenuCommands = ["es", "entrysounds"]
DefaultVolume = 20
VolumeOptions = [0, 20, 40, 60, 80, 100]

[EntrySounds."1"]
Sound = ""
Flags = []
SteamID = ""
JoinMessage = "{gold}{playername}{blue} joined the server!"
MessageType = "chat" # chat,alert,center
```