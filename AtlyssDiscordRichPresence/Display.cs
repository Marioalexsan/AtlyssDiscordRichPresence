namespace Marioalexsan.AtlyssDiscordRichPresence;

using BepInEx.Configuration;
using static States;

public class Display
{
    public enum DisplayPresets
    {
        Custom,
        Default,
        Detailed,
        DetailedPercentages,
        Pon,
        Emojis
    }

    public enum Texts
    {
        // Full texts
        PlayerAlive,
        PlayerDead,
        MainMenu,
        CharacterCreation,
        CharacterCreationDetails,
        Exploring,
        Idle,
        FightingInArena,
        FightingBoss,
        DungeonBossEnd,
        Singleplayer,
        Multiplayer,

        // Race variables
        RaceUnknown,
        RaceImp,
        RacePoon,
        RaceKubold,
        RaceByrdle,
        RaceChang,

        // Class variables
        ClassUnknown,
        ClassNovice,
        ClassFighter,
        ClassMystic,
        ClassBandit,

        // Class tier variables for Fighter
        ClassTierBerserker,
        ClassTierPaladin,

        // Class tier variables for Mystic
        ClassTierMagus,
        ClassTierBishop,

        // Class tier variables for Bandit
        ClassTierRogue,
        ClassTierEngineer
    }

    public Display(ConfigFile config)
    {
        void RegisterCustomPresetText(Texts text, string description)
        {
            _customPresetData[text] = config.Bind("Display", text.ToString(), GetDefaultPresetText(text), description);
        }

        DisplayPreset = config.Bind("General", nameof(DisplayPreset), DisplayPresets.Custom, "Preset to use for texts. \"Custom\" will use the custom texts defined in the config.");

        // Regular texts
        RegisterCustomPresetText(Texts.PlayerAlive, "Text to display for player stats while alive.");
        RegisterCustomPresetText(Texts.PlayerDead, "Text to display for player stats while dead.");
        RegisterCustomPresetText(Texts.MainMenu, "Text to display while you're in the main menu.");
        RegisterCustomPresetText(Texts.CharacterCreation, "Text to display while you're in the character creation screen.");
        RegisterCustomPresetText(Texts.CharacterCreationDetails, "Text to display about the current character you're creating in the creation screen.");
        RegisterCustomPresetText(Texts.Exploring, "Text to display while exploring the world.");
        RegisterCustomPresetText(Texts.Idle, "Text to display while being idle in the world.");
        RegisterCustomPresetText(Texts.FightingInArena, "Text to display while an arena is active.");
        RegisterCustomPresetText(Texts.FightingBoss, "Text to display while a boss is active.");
        RegisterCustomPresetText(Texts.DungeonBossEnd, "Text to display during the post-boss state of a dungeon.");
        RegisterCustomPresetText(Texts.Singleplayer, "Text to display while in singleplayer.");
        RegisterCustomPresetText(Texts.Multiplayer, "Text to display while in multiplayer.");

        // Race names
        RegisterCustomPresetText(Texts.RaceImp, $"Text to use when displaying {PLAYERRACE} (for Imps).");
        RegisterCustomPresetText(Texts.RacePoon, $"Text to use when displaying {PLAYERRACE} (for Poons).");
        RegisterCustomPresetText(Texts.RaceKubold, $"Text to use when displaying {PLAYERRACE} (for Kubolds).");
        RegisterCustomPresetText(Texts.RaceByrdle, $"Text to use when displaying {PLAYERRACE} (for Byrdles).");
        RegisterCustomPresetText(Texts.RaceChang, $"Text to use when displaying {PLAYERRACE} (for Changs).");

        // Class names
        RegisterCustomPresetText(Texts.ClassNovice, $"Text to use when displaying {PLAYERCLASS} (for Novices).");
        RegisterCustomPresetText(Texts.ClassFighter, $"Text to use when displaying {PLAYERCLASS} (for Fighters).");
        RegisterCustomPresetText(Texts.ClassMystic, $"Text to use when displaying {PLAYERCLASS} (for Mystics).");
        RegisterCustomPresetText(Texts.ClassBandit, $"Text to use when displaying {PLAYERCLASS} (for Bandits).");

        // Class tier names
        RegisterCustomPresetText(Texts.ClassTierBerserker, $"Text to use when displaying {PLAYERCLASS} (for Berserkers - Fighter tier).");
        RegisterCustomPresetText(Texts.ClassTierPaladin, $"Text to use when displaying {PLAYERCLASS} (for Paladins - Fighter tier).");
        RegisterCustomPresetText(Texts.ClassTierMagus, $"Text to use when displaying {PLAYERCLASS} (for Magi - Mystic tier).");
        RegisterCustomPresetText(Texts.ClassTierBishop, $"Text to use when displaying {PLAYERCLASS} (for Bishops - Mystic tier).");
        RegisterCustomPresetText(Texts.ClassTierRogue, $"Text to use when displaying {PLAYERCLASS} (for Rogues - Bandit tier).");
        RegisterCustomPresetText(Texts.ClassTierEngineer, $"Text to use when displaying {PLAYERCLASS} (for Engineers - Bandit tier).");
    }

    public ConfigEntry<DisplayPresets> DisplayPreset { get; }

    private readonly Dictionary<Texts, ConfigEntry<string>> _customPresetData  = [];

    private static string EscapeVars(string input)
    {
        return input.Replace("@", "@0").Replace("{", "@1").Replace("}", "@2");
    }

    private static string EscapeText(string input)
    {
        return input.Replace("@", "@0");
    }

    private static string Unescape(string input)
    {
        return input.Replace("@2", "}").Replace("@1", "{").Replace("@0", "@");
    }

    public string ReplaceVars(string input, GameState state)
    {
        input = EscapeText(input);

        foreach ((var key, var value) in state.GetStates())
        {
            input = input.Replace($"{{{key}}}", EscapeVars(GetVariable(key, value())));
        }

        input = Unescape(input);

        return input;
    }

    public string GetText(Texts text, GameState state) => ReplaceVars(GetRawText(text), state);

    private string GetVariable(string variable, string value) => variable switch
    {
        PLAYERCLASS => GetMappedText(value),
        PLAYERRACE => GetMappedText(value),
        CHARCREATERACE => GetMappedText(value),
        PLAYERRACEANDCLASS => string.Join(" ", value.Split(" ").Select(GetMappedText)),
        _ => value
    };

    private string GetMappedText(string str) => str.ToLower() switch
    {
        "imp" => GetRawText(Texts.RaceImp),
        "poon" => GetRawText(Texts.RacePoon),
        "kubold" => GetRawText(Texts.RaceKubold),
        "byrdle" => GetRawText(Texts.RaceByrdle),
        "chang" => GetRawText(Texts.RaceChang),
        "novice" => GetRawText(Texts.ClassNovice),
        "fighter" => GetRawText(Texts.ClassFighter),
        "mystic" => GetRawText(Texts.ClassMystic),
        "bandit" => GetRawText(Texts.ClassBandit),
        "berserker" => GetRawText(Texts.ClassTierBerserker),
        "paladin" => GetRawText(Texts.ClassTierPaladin),
        "magus" => GetRawText(Texts.ClassTierMagus),
        "bishop" => GetRawText(Texts.ClassTierBishop),
        "rogue" => GetRawText(Texts.ClassTierRogue),
        "engineer" => GetRawText(Texts.ClassTierEngineer),
        _ => str
    };

    private string GetRawText(Texts text) => DisplayPreset.Value switch
    {
        DisplayPresets.Custom => GetCustomPresetText(text),
        DisplayPresets.Default => GetDefaultPresetText(text),
        DisplayPresets.Detailed => GetDetailedPresetText(text),
        DisplayPresets.DetailedPercentages => GetDetailedPercentagesPresetText(text),
        DisplayPresets.Pon => GetPonPresetText(text),
        DisplayPresets.Emojis => GetEmojisPresetText(text),
        _ => "[Unknown Preset]"
    };

    private string GetCustomPresetText(Texts text)
    {
        if (_customPresetData.TryGetValue(text, out var config))
            return config.Value;

        return GetDefaultPresetText(text);
    }

    private string GetDefaultPresetText(Texts text) => text switch
    {
        Texts.PlayerAlive => $"Lv{{{LVL}}} {{{PLAYERRACEANDCLASS}}} ({{{HP}}}/{{{MAXHP}}} HP)",
        Texts.PlayerDead => $"Lv{{{LVL}}} {{{PLAYERRACEANDCLASS}}} (Fainted)",
        Texts.MainMenu => $"In Main Menu",
        Texts.CharacterCreation => $"In Character Creation",
        Texts.CharacterCreationDetails => $"Creating {{{CHARCREATERACE}}} named \"{{{CHARCREATENAME}}}\"",
        Texts.Exploring => $"{{{PLAYERNAME}}} exploring {{{WORLDAREA}}}",
        Texts.Idle => $"{{{PLAYERNAME}}} idle in {{{WORLDAREA}}}",
        Texts.FightingInArena => $"{{{PLAYERNAME}}} fighting in {{{WORLDAREA}}}",
        Texts.FightingBoss => $"{{{PLAYERNAME}}} fighting {{{BOSSNAME}}} in {{{WORLDAREA}}}",
        Texts.DungeonBossEnd => $"{{{PLAYERNAME}}} getting boss loot in {{{WORLDAREA}}}",
        Texts.Singleplayer => $"Singleplayer",
        Texts.Multiplayer => $"Multiplayer on {{{SERVERNAME}}} ({{{PLAYERS}}}/{{{MAXPLAYERS}}})",
        Texts.RaceUnknown => "[Unknown Race]",
        Texts.RaceImp => "Imp",
        Texts.RacePoon => "Poon",
        Texts.RaceKubold => "Kubold",
        Texts.RaceByrdle => "Byrdle",
        Texts.RaceChang => "Chang",
        Texts.ClassUnknown => "[Unknown Class]",
        Texts.ClassNovice => "Novice",
        Texts.ClassFighter => "Fighter",
        Texts.ClassMystic => "Mystic",
        Texts.ClassBandit => "Bandit",
        Texts.ClassTierBerserker => "Berserker",
        Texts.ClassTierPaladin => "Paladin",
        Texts.ClassTierMagus => "Magus",
        Texts.ClassTierBishop => "Bishop",
        Texts.ClassTierRogue => "Rogue",
        Texts.ClassTierEngineer => "Engineer",
        _ => "[No Text]"
    };

    private string GetDetailedPresetText(Texts text) => text switch
    {
        Texts.PlayerAlive => $"Lv{{{LVL}}} {{{PLAYERRACEANDCLASS}}} ({{{EXP}}}/{{{EXPNEXT}}} EXP) ({{{HP}}}/{{{MAXHP}}} HP) ({{{MP}}}/{{{MAXMP}}} MP)",
        Texts.PlayerDead => $"Lv{{{LVL}}} {{{PLAYERRACEANDCLASS}}} (Fainted) in {{{WORLDAREA}}}",
        _ => GetDefaultPresetText(text)
    };

    private string GetDetailedPercentagesPresetText(Texts text) => text switch
    {
        Texts.PlayerAlive => $"Lv{{{LVL}}} {{{PLAYERRACEANDCLASS}}} ({{{EXPPCT}}}%) ({{{HPPCT}}}%) ({{{MPPCT}}}%)",
        Texts.PlayerDead => $"Lv{{{LVL}}} {{{PLAYERRACEANDCLASS}}} (Fainted) in {{{WORLDAREA}}}",
        _ => GetDefaultPresetText(text)
    };

    private string GetPonPresetText(Texts text) => text switch
    {
        Texts.RaceImp => "Inp",
        Texts.RacePoon => "Pon",
        Texts.RaceKubold => "Cubol",
        Texts.RaceByrdle => "Birb",
        Texts.RaceChang => "Tang",
        _ => GetDefaultPresetText(text)
    };

    private string GetEmojisPresetText(Texts text) => text switch
    {
        Texts.PlayerAlive => $"{{{PLAYERRACE}}}{{{PLAYERCLASS}}} 🏆{{{LVL}}} ❤️{{{HP}}} ✨{{{MP}}}⚡{{{SP}}}",
        Texts.PlayerDead => $"{{{PLAYERRACE}}}{{{PLAYERCLASS}}} 🏆{{{LVL}}} 💀",
        Texts.MainMenu => $"📖 Main Menu",
        Texts.CharacterCreation => $"📖 Character Creation",
        Texts.CharacterCreationDetails => $"📖 Creating {{{CHARCREATERACE}}} \"{{{CHARCREATENAME}}}\"",
        Texts.Exploring => $"{{{PLAYERNAME}}} 🌎{{{WORLDAREA}}}",
        Texts.Idle => $"{{{PLAYERNAME}}} 🌎{{{WORLDAREA}}} 💤",
        Texts.FightingInArena => $"{{{PLAYERNAME}}} 🌎{{{WORLDAREA}}} ⚔️",
        Texts.FightingBoss => $"{{{PLAYERNAME}}} 🌎{{{WORLDAREA}}} ⚔️👿 {{{BOSSNAME}}}",
        Texts.DungeonBossEnd => $"{{{PLAYERNAME}}} 🌎{{{WORLDAREA}}} ⚔️👿👑",
        Texts.Singleplayer => $"👤Solo",
        Texts.Multiplayer => $"👥{{{SERVERNAME}}} ({{{PLAYERS}}}/{{{MAXPLAYERS}}})",
        Texts.RaceUnknown => GetDefaultPresetText(text),
        Texts.RaceImp => "👿", // Angry face with horns
        Texts.RacePoon => "🐰", // Rabbit
        Texts.RaceKubold => "🐲", // Dragon face
        Texts.RaceByrdle => "🐦", // Bird
        Texts.RaceChang => "🐿️", // Chipmunk
        Texts.ClassUnknown => GetDefaultPresetText(text),
        Texts.ClassNovice => "🌿", // Herb
        Texts.ClassFighter => "🪓", // Axe
        Texts.ClassMystic => "🌀", // Cyclone
        Texts.ClassBandit => "👑", // Crown
        Texts.ClassTierBerserker => "🐗", // Boar
        Texts.ClassTierPaladin => "🛡️", // Shield
        Texts.ClassTierMagus => "🪄", // Magic wand
        Texts.ClassTierBishop => "♗", // Bishop chess piece
        Texts.ClassTierRogue => "🥷", // Ninja
        Texts.ClassTierEngineer => "🔧", // Wrench
        _ => GetDefaultPresetText(text)
    };
}
