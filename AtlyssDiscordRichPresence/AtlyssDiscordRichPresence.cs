using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using DiscordRPC.Registry;
using HarmonyLib;
using Marioalexsan.AtlyssDiscordRichPresence.HarmonyPatches;
using Marioalexsan.AtlyssDiscordRichPresence.SoftDependencies;
using Steamworks;
namespace Marioalexsan.AtlyssDiscordRichPresence;

[BepInPlugin(ModInfo.GUID, ModInfo.NAME, ModInfo.VERSION)]
[BepInDependency(EasySettings.ModID, BepInDependency.DependencyFlags.SoftDependency)]
public class AtlyssDiscordRichPresence : BaseUnityPlugin
{
    public enum JoinEnableSetting
    {
        JoiningDisabled,
        PublicOnly,
        PublicAndFriends,
        All,
    }

    // The default app hosts the icons and other data used for Rich Presence
    // You can use a custom app ID if you want to customize the icons
    public const string DefaultDiscordAppId = "1309967280842735738";

    public static AtlyssDiscordRichPresence Plugin => _plugin ?? throw new InvalidOperationException($"{nameof(AtlyssDiscordRichPresence)} hasn't been initialized yet. Either wait until initialization, or check via ChainLoader instead.");
    private static AtlyssDiscordRichPresence? _plugin;

    internal new ManualLogSource Logger { get; private set; }

    private readonly Display _display;
    private readonly GameState _state = new();

    private readonly RichPresenceWrapper _richPresence;
    private readonly Harmony _harmony;

    public ConfigEntry<bool> ModEnabled { get; private set; }
    public ConfigEntry<JoinEnableSetting> ServerJoinSetting { get; private set; }
    public ConfigEntry<string> DiscordAppId { get; private set; }
    public ConfigEntry<RichPresenceWrapper.LogLevels> DiscordRPCLogLevel { get; private set; }

    // Used to join a lobby as soon as we're ready for it
    private ulong? QueuedSteamLobbyId;

    public AtlyssDiscordRichPresence()
    {
        _plugin = this;
        Logger = base.Logger;
        _harmony = new Harmony(ModInfo.GUID);

        _display = new(Config);
        ModEnabled = Config.Bind("General", "Enable", true, "Enable or disable this mod. While disabled, no in-game data is shown.");
        ServerJoinSetting = Config.Bind("General", "ServerJoinSetting", JoinEnableSetting.PublicAndFriends, "Set the server privacy levels for which Discord should allow joining.");
        DiscordRPCLogLevel = Config.Bind("General", "DiscordRPCLogLevel", RichPresenceWrapper.LogLevels.Warning, "Log level to use for logs coming from the Discord RPC library.");
        DiscordAppId = Config.Bind("Advanced", "DiscordAppId", DefaultDiscordAppId, "The Discord application ID to be used by the mod. ***Do not change this unless you know what you're doing!***");

        _richPresence = new(DiscordAppId.Value, Logger, DiscordRPCLogLevel.Value);
        _richPresence.OnJoin += OnJoinLobby;

        if (WineDetect.IsRunningInWine)
        {
            Logging.LogInfo("Wine detected!");
            Logging.LogInfo($"Wine version: {WineDetect.WineVersion}");
            Logging.LogInfo($"System name: {WineDetect.SystemName}");
            Logging.LogInfo($"System version: {WineDetect.SystemVersion}");
        }
        else
        {
            Logging.LogInfo("Wine not detected!");
        }
    }

    private void OnJoinLobby(object sender, RichPresenceWrapper.JoinData e)
    {
        Logging.LogInfo($"OnJoinLobby " + e.Id);

        string actualId = e.Id;

        if (actualId.StartsWith("secret_"))
            actualId = actualId["secret_".Length..];

        if (!ulong.TryParse(actualId, out ulong steamId))
        {
            Logging.LogInfo($"ID is invalid, can't join.");
            return;
        }

        QueuedSteamLobbyId = steamId;
    }

    private enum TimerTrackerState
    {
        MainMenu,
        ExploringWorld
    }

    private TimerTrackerState _timeTrackerState;

    private void UpdatePresence(PresenceData data, TimerTrackerState state)
    {
        _richPresence.SetPresence(data, _timeTrackerState != state);
        _timeTrackerState = state;
    }

    private void Update()
    {
        if (!MainMenuManager._current)
            return;

        if (QueuedSteamLobbyId.HasValue)
        {
            var lobbyId = QueuedSteamLobbyId.Value;
            QueuedSteamLobbyId = null;

            SteamLobby._current.Init_LobbyJoinRequest(new Steamworks.CSteamID(lobbyId));
        }

        if (MainMenuManager._current._mainMenuCondition != MainMenuCondition.In_Game)
        {
            UpdateMainMenuPresence(MainMenuManager._current);
        }
        else
        {
            if (
                (bool)AtlyssNetworkManager._current &&
                !AtlyssNetworkManager._current._soloMode &&
                !(AtlyssNetworkManager._current._steamworksMode && !SteamManager.Initialized) &&
                (bool)Player._mainPlayer
                )
            {
                _state.InMultiplayer = true;
                _state.Players = FindObjectsOfType<Player>().Length;
                _state.MaxPlayers = ServerInfoObject._current._maxConnections;
                _state.ServerName = ServerInfoObject._current._serverName;
                _state.ServerJoinId = SteamLobby._current._currentLobbyID.ToString();
            }
            else
            {
                _state.InMultiplayer = false;
                _state.Players = 1;
                _state.MaxPlayers = 1;
                _state.ServerJoinId = "";
            }

            UpdateWorldAreaPresence(Player._mainPlayer, null);
            CheckCombatState();
        }

        _richPresence.Tick();
    }

    private void Awake()
    {
        InitializeConfiguration();
        _harmony.PatchAll();

        _richPresence.Enabled = ModEnabled.Value;
    }

    private void InitializeConfiguration()
    {
        if (EasySettings.IsAvailable)
        {
            EasySettings.OnApplySettings.AddListener(() =>
            {
                try
                {
                    Config.Save();

                    _richPresence.LogLevel = DiscordRPCLogLevel.Value;
                    _richPresence.Enabled = ModEnabled.Value;
                }
                catch (Exception e)
                {
                    Logging.LogError($"AtlyssDiscordRichPresence crashed in OnApplySettings! Please report this error to the mod developer:");
                    Logging.LogError(e.ToString());
                }
            });
            EasySettings.OnInitialized.AddListener(() =>
            {
                // DiscordAppId is not included on purpose 
                EasySettings.AddHeader("Atlyss Discord Rich Presence");
                EasySettings.AddToggle("Enabled", ModEnabled);
                EasySettings.AddDropdown("Enable server joining in Discord", ServerJoinSetting);
                EasySettings.AddDropdown("DiscordRPC log level", DiscordRPCLogLevel);
                EasySettings.AddDropdown("Rich Presence display preset", _display.DisplayPreset);
                // TODO: Text inputs on EasySettings aren't supported (yet?), so we can't configure custom presets here
            });
        }
    }

    private void OnDestroy()
    {
        _richPresence.Dispose();
    }

    internal void CheckCombatState()
    {
        _state.InArenaCombat = false;
        _state.InBossCombat = false;
        _state.InPostBoss = false;
        _state.BossName = "";

        if (Player._mainPlayer && Player._mainPlayer._playerMapInstance)
        {
            if (Player._mainPlayer._playerMapInstance._patternInstance)
            {
                // Dungeon arena stuff

                var inst = Player._mainPlayer._playerMapInstance._patternInstance;

                bool isAction = false;

                for (int i = 0; i < inst._setInstanceCreepArenas.Count; i++)
                {
                    var arena = inst._setInstanceCreepArenas[i];
                    if (arena._arenaEnabled && arena._creepSpawnerObject._playersWithinSpawnerRadius.Contains(Player._mainPlayer))
                    {
                        isAction = true;
                        break;
                    }
                }

                bool isBoss = inst._isBossEngaged && inst._bossSpawner && inst._bossSpawner._playersWithinSpawnerRadius.Contains(Player._mainPlayer);
                bool postBoss = inst._isBossDefeated;

                _state.InArenaCombat = isAction;
                _state.InBossCombat = isBoss;
                _state.InPostBoss = postBoss;

                if (inst._bossSpawner && inst._bossSpawner._spawnedCreeps.Count > 0)
                {
                    _state.BossName = inst._bossSpawner._spawnedCreeps[0].Network_creepDisplayName;
                }
            }
            else
            {
                // World map stuff

                Creep? possibleBossCreep = null;

                foreach (var creep in TrackedAggroCreeps.Creeps)
                {
                    if (creep != null && creep.Network_aggroedEntity && creep._scriptCreep._playMapInstanceActionMusic)
                    {
                        if (possibleBossCreep == null || creep._creepLevel > possibleBossCreep._creepLevel)
                            possibleBossCreep = creep;
                    }
                }

                if (possibleBossCreep != null)
                {
                    _state.InBossCombat = true;
                    _state.BossName = possibleBossCreep._creepDisplayName;
                }
            }
        }

        UpdateWorldAreaPresence(null, null);
    }

    internal void Player_OnPlayerMapInstanceChange(Player self, MapInstance _new)
    {
        if (self == Player._mainPlayer)
        {
            _state.InArenaCombat = false;
            _state.InBossCombat = false;
            UpdateWorldAreaPresence(self, _new);
        }
    }

    private void UpdateMainMenuPresence(MainMenuManager manager)
    {
        if (manager._mainMenuCondition == MainMenuCondition.CharacterCreation)
        {
            var charManager = manager._characterCreationManager;

            if (!charManager)
                return;

            var characterName = charManager._characterNameInputField?.text;

            if (string.IsNullOrEmpty(characterName))
                characterName = "";

            _state.CharacterCreationName = characterName;

            var raceIndex = charManager._currentRaceSelected;

            if (0 <= raceIndex && raceIndex < charManager._raceDisplayModels.Length)
            {
                _state.CharacterCreationRace = charManager._raceDisplayModels[raceIndex]?._scriptablePlayerRace?._raceName ?? "";
            }
            else
            {
                _state.CharacterCreationRace = "";
            }

            UpdatePresence(new()
            {
                Details = _display.GetText(Display.Texts.CharacterCreation, _state),
                State = _display.GetText(Display.Texts.CharacterCreationDetails, _state),
                LargeImageKey = Assets.ATLYSS_ICON,
                LargeImageText = "ATLYSS"
            }, TimerTrackerState.MainMenu);
        }
        else
        {
            UpdatePresence(new()
            {
                Details = _display.GetText(Display.Texts.MainMenu, _state),
                LargeImageKey = Assets.ATLYSS_ICON,
                LargeImageText = "ATLYSS"
            }, TimerTrackerState.MainMenu);
        }
    }

    private void UpdateWorldAreaPresence(Player? player, MapInstance? area)
    {
        if (player != null)
            _state.UpdateData(player);

        if (area != null)
            _state.UpdateData(area);

        var details = _display.GetText(Display.Texts.Exploring, _state);

        if (_state.IsIdle)
            details = _display.GetText(Display.Texts.Idle, _state);

        if (_state.InPostBoss)
            details = _display.GetText(Display.Texts.DungeonBossEnd, _state);

        else if (_state.InBossCombat)
            details = _display.GetText(Display.Texts.FightingBoss, _state);

        else if (_state.InArenaCombat)
            details = _display.GetText(Display.Texts.FightingInArena, _state);

        var state = _display.GetText(Display.Texts.PlayerAlive, _state);

        if (_state.HealthPercentage <= 0)
            state = _display.GetText(Display.Texts.PlayerDead, _state);

        UpdatePresence(new()
        {
            Details = details,
            State = state,
            LargeImageKey = !_state.InMultiplayer ? Assets.ATLYSS_SINGLEPLAYER : Assets.ATLYSS_MULTIPLAYER,
            LargeImageText = !_state.InMultiplayer ? _display.GetText(Display.Texts.Singleplayer, _state) : _display.GetText(Display.Texts.Multiplayer, _state),
            SmallImageKey = MapAreaToIcon(_state.WorldAreaType),
            SmallImageText = _state.WorldArea,
            Multiplayer = !_state.InMultiplayer ? null : new ServerData()
            {
                Id = _state.ServerJoinId,
                Size = _state.Players,
                Max = _state.MaxPlayers,
                AllowJoining = CanJoinMultiplayer()
            }
        }, TimerTrackerState.ExploringWorld);
    }

    private bool CanJoinMultiplayer()
    {
        // 0 - Public / 1 - Friends Only / 2 - Private
        var lobbyType = LobbyListManager._current._lobbyTypeDropdown.value;

        return lobbyType switch
        {
            0 => ServerJoinSetting.Value >= JoinEnableSetting.PublicOnly,
            1 => ServerJoinSetting.Value >= JoinEnableSetting.PublicAndFriends,
            2 => ServerJoinSetting.Value >= JoinEnableSetting.All,
            _ => false
        };
    }

    private static string MapAreaToIcon(ZoneType area) => area switch
    {
        ZoneType.Safe => Assets.ZONESELECTIONICON_SAFE,
        ZoneType.Field => Assets.ZONESELECTIONICON_FIELD,
        ZoneType.Dungeon => Assets.ZONESELECTIONICON_DUNGEON,
        ZoneType.Arena => Assets.ZONESELECTIONICON_ARENA,
        _ => Assets.ZONESELECTIONICON_FIELD
    };
}