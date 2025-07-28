# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.3.0] - 2025-Jul-29

### Added

- Added support for boss names with the `BOSSNAME` variable
  - Also added boss names to the boss fight state text
  - Custom presets need to be updated to use the new boss names
- Added basic support for world map boss fights, such as Slime Diva
  - Future / modded bosses will be considered as a boss fight if `_playMapInstanceActionMusic` is set to true, and `Network_aggroedEntity` is set to a valid entity
- Added support for character creation display state
  - It will show the name and race of the character you're creating using `CHARCREATENAME` and `CHARCREATERACE` variables
- Added support for a post-dungeon boss state display
- Added support for class tiers (Paladin, Bishop, Rogue, etc.)
  - They act as separate classes and will be shown with `PLAYERCLASS`
- Added three presets
  - Detailed - same as Default, but shows more player stats by default
  - DetailedPercentages - same as Detailed, but uses percentage values instead
  - Pon - same as Default, but player race names are funnier

### Fixed

- Fixed an issue with boss fight state persisting after leaving the dungeon
- World zone type is now determined based on zone type instead of being hardcoded for each vanilla zone
  - This should allow modded maps to display their type correctly

## [1.2.0] - 2025-May-02

### Added

- Added support for EasySettings, which can be used to toggle some features while in-game
- You can now select between three presets for Rich Presence text: Custom, Default and Emojis
  - Custom uses the custom texts defined in the config file
  - Default is a the default text preset used for the initial values in Custom
  - Emojis is a preset that replaces most of the words with emojis
- Added configuration options for race and class name variables ({PLAYERRACE} and {PLAYERCLASS}), allowing you to customize how races are displayed
  - For example, you could change "Poon" to display "funny rabbit thing" instead
- Added a configuration option for logs coming from the Discord RPC integration
- You can now send game server invites to each other
  - To be able to join a server, both users must be in-game, and both of them must have the mod for it to function correctly
  - You can configure whenever the join button is available by using the ServerJoinSetting config option. The default allows joining Public and Friends games
  - Currently it's not possible to join games without both players having the mod, or while the game is not open for the person accepting an invite

### Changed

- Changed presence update interval to 3 seconds (up from 1)
  - Hopefully should smooth out updates and reduce rate limiting issues

### Fixed

- Fixed idle state text not appearing when the player is idle for some time

## [1.1.0] - 2024-Dec-03

### Added

- You can now customize the strings displayed via the mod's configuration file (`BepInEx/config/Marioalexsan.AtlyssDiscordRichPresence.cfg`)
- Added small icons for dungeons, arenas, field zones and safe zones
- Added custom big icons for singleplayer and multiplayer
- Added display for server name and player count when playing in multiplayer

### Changed

- Modified the default display of strings so that it's more compact

## [1.0.0] - 2024-Nov-28

### Changed

**Initial mod release**