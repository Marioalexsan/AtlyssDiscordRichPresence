# Atlyss Discord Rich Presence

Enables Discord Rich Presence support for ATLYSS.

![](https://i.imgur.com/eGlh4yG.png)

Shows various stats, such as:
- Your current status (in menu, exploring a zone, etc.)
  - Main menu
  - Character creation
  - World areas
  - Fights in dungeons
  - Boss fights (including world bosses such as Slime Diva)
- Your character and their current state
- Whenever you're in singleplayer or multiplayer, and details of the server
- Elapsed playtime

Also allows you to:
- Customize the texts displayed by the integration, and select between available presets
- Send invites for the ATLYSS server you're in to people who also have the mod (either by launching or joining while in-game)
- Have people who also have the mod be able to join your ATLYSS server (either by launching or joining while in-game)

# Configuration

You can configure the displayed strings via the configuration file in `BepInEx/config/Marioalexsan.AtlyssDiscordRichPresence.cfg`.

Within these strings, you can use various variables to display stats, such as `{PLAYERNAME} exploring {WORLDAREA}` => `Chip exploring Sanctum`. You can use this to display other stats, such as Health percentage instead of exact health numbers.

The available variables are as follows:
- `HP` - Player health
- `MAXHP` - Player maximum health
- `HPPCT` - Player health pecentage (0-100)
- `MP` - Player mana
- `MAXMP` - Player maximum mana
- `MPPCT` - Player mana pecentage (0-100)
- `SP` - Player stamina
- `MAXSP` - Player mximum stamina
- `SPPCT` - Player stamina pecentage (0-100)
- `LVL` - Player level
- `EXP` - Player experience
- `EXPNEXT` - Player max experience (i.e. the experience required to level up)
- `EXPPCT` - Player experience percentage (0-100)
- `PLAYERNAME` - Player display name
- `PLAYERRACE` - Player race ("Poon", etc.)
- `PLAYERCLASS` - Player class ("Novice", "Fighter", etc.)
- `PLAYERRACEANDCLASS` - Displays both race and class together ("Poon Novice", etc.)
- `WORLDAREA` - Current world area ("Sanctum", etc.)
- `BOSSNAME` - Current boss name ("Lord Kaluuz", etc.)
- `SERVERNAME` - The server you're playing on
- `PLAYERS` - The number of players in the server you're on
- `MAXPLAYERS` - The maximum number of players in the server you're on
- `CHARCREATENAME` - The name of the current character in the Character Creation screen
- `CHARCREATERACE` - The race of the current character in the Character Creation screen

You can also further configure how player races (PLAYERRACE) and player classes (PLAYERCLASS) are displayed by configuring variables such as `RacePoon`.

# Linux Compatibility

To play with this mod on Linux, you'll need to download and use [rpc-bridge](https://github.com/EnderIce2/rpc-bridge) to allow for Rich Presence support.

In ATLYSS's Steam settings, General tab, Launch options, you'll have to enter the following text, where `/path/to/bridge.sh` is the path to the downloaded script:

```
/path/to/bridge.sh %command%
```

You will also have to make `bridge.sh` executable by opening a terminal in the containing folder, and running the following command: 

```
chmod +x bridge.sh
```

Support for launching Atlyss and joining servers via "Join Game" is experimental due to requiring workarounds for Proton. Some things to keep in mind:
- The mod will create an entry under `~/.local/share/applications/discord-{AppId}.desktop` to store a launch option. This is likely going to appear as "Atlyss" in your desktop environment's application menu.
- The mod will backup your `~/.config/mimeapps.list` configuration into `~/.config/mimeapps.list.atlyssbackup`, then manually edit it to include the necessary MIME associations for Discord to be able to launch the game

# Mod Compatibility

AtlyssDiscordRichPresence targets the following game versions and mods:

- ATLYSS 72025.a8
- Nessie's EasySettings 1.1.8 (optional dependency used for configuration)

Compatibility with other game versions and mods is not guaranteed, especially for updates with major changes.

# Notes

- When changing your BepInEx install path (for example by switching r2modman profiles), or changing your game install path, you have to relaunch the modded game outside of Discord so that its launch information is updated correctly

# Gallery

![](https://i.imgur.com/zHNFQb4.png)

![](https://i.imgur.com/G2VhZDa.png)

