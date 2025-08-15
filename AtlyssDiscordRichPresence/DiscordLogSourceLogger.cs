using BepInEx.Logging;
using DiscordRPC.Logging;

namespace Marioalexsan.AtlyssDiscordRichPresence;

public class DiscordLogSourceLogger(ManualLogSource source, DiscordRPC.Logging.LogLevel level) : ILogger
{
    public DiscordRPC.Logging.LogLevel Level { get; set; } = level;

    public void Error(string message, params object[] args) => LogToBepInEx(DiscordRPC.Logging.LogLevel.Error, message, args);

    public void Warning(string message, params object[] args) => LogToBepInEx(DiscordRPC.Logging.LogLevel.Warning, message, args);

    public void Info(string message, params object[] args) => LogToBepInEx(DiscordRPC.Logging.LogLevel.Info, message, args);

    public void Trace(string message, params object[] args) => LogToBepInEx(DiscordRPC.Logging.LogLevel.Trace, message, args);

    private void LogToBepInEx(DiscordRPC.Logging.LogLevel messageLevel, string message, params object[] args)
    {
        messageLevel = ChangeLogLevel(message, messageLevel);

        if (Level > messageLevel)
            return;

        switch (messageLevel)
        {
            case DiscordRPC.Logging.LogLevel.Trace:
                source.LogDebug($"[DiscordRPC:Trace] {string.Format(message, args)}");
                break;
            case DiscordRPC.Logging.LogLevel.Info:
                source.LogInfo($"[DiscordRPC:Info] {string.Format(message, args)}");
                break;
            case DiscordRPC.Logging.LogLevel.Warning:
                source.LogWarning($"[DiscordRPC:Warning] {string.Format(message, args)}");
                break;
            case DiscordRPC.Logging.LogLevel.Error:
                source.LogError($"[DiscordRPC:Error] {string.Format(message, args)}");
                break;
        }
    }

    private DiscordRPC.Logging.LogLevel ChangeLogLevel(string message, DiscordRPC.Logging.LogLevel originalLevel)
    {
        // Filter out spammy junk

        if (message.Contains("Failed to connect for some reason."))
            return DiscordRPC.Logging.LogLevel.None;

        if (message.Contains("Failed connection to {0}. {1}"))
            return DiscordRPC.Logging.LogLevel.None;

        if (message.Contains("Attempting to connect to '{0}'"))
            return DiscordRPC.Logging.LogLevel.None;

        if (message.Contains("Tried to close a already closed pipe."))
            return DiscordRPC.Logging.LogLevel.None;

        // Change the levelToUse of some other spammy junk

        if (message.Contains("Handling Response. Cmd: {0}, Event: {1}"))
            return DiscordRPC.Logging.LogLevel.Trace;

        return originalLevel;
    }
}
