using DiscordRPC.Logging;
using DiscordRPC.Registry;
using Marioalexsan.AtlyssDiscordRichPresence;
using System.Diagnostics;

internal class ProtonUnixUriSchemeCreator(ILogger logger) : IUriSchemeCreator
{
    private readonly ILogger logger = logger;

    public bool RegisterUriScheme(UriSchemeRegister register)
    {
        // We don't have a proper HOME variable, so use this instead
        var username = Environment.GetEnvironmentVariable("WINEUSERNAME");

        if (string.IsNullOrEmpty(username))
        {
            logger.Error("Failed to register because the WINEUSERNAME variable was not set.");
            return false;
        }

        var home = $"/home/{username}";

        string exe = register.ExecutablePath;

        if (string.IsNullOrEmpty(exe))
        {
            logger.Error("Failed to register because the application was not located.");
            return false;
        }

        string command = register.UsingSteamApp ? $"xdg-open steam://rungameid/{register.SteamAppID}" : exe;

        // I have no idea why discord-rpc uses %u in Exec, but it breaks things so it goes away
        string file =
            $"""
            [Desktop Entry]
            Name=Game {register.ApplicationID}
            Exec={command}
            Type=Application
            NoDisplay=true
            Categories=Discord;Games;
            MimeType=x-scheme-handler/discord-{register.ApplicationID}
            """.Replace("\\", "/");

        string filename = $"discord-{register.ApplicationID}.desktop";
        string filepath = $"Z:/{home}/.local/share/applications";
        var directory = Directory.CreateDirectory(filepath);
        if (!directory.Exists)
        {
            logger.Error("Failed to register because {0} does not exist", filepath);
            return false;
        }

        File.WriteAllText(Path.Combine(filepath, filename), file);

        if (!RegisterMime(home, register.ApplicationID))
        {
            logger.Error("Failed to register because the Mime failed.");
            return false;
        }

        logger.Trace("Registered {0}, {1}, {2}", Path.Combine(filepath, filename), file, command);
        return true;
    }

    private bool RegisterMime(string homeDir, string appid)
    {
        //var shellCmd = $"xdg-mime default discord-{appid}.desktop x-scheme-handler/discord-{appid}";
        var mimeappEntry = $"x-scheme-handler/discord-{appid}=discord-{appid}.desktop";

        var mimeappsPath = $"Z:/{homeDir}/.config/mimeapps.list";
        var mimeappsPathBackup = $"Z:/{homeDir}/.config/mimeapps.list.atlyssbackup";

        if (!File.Exists(mimeappsPath))
        {
            logger.Error($"Couldn't find .config/mimeapps.list to update.");
            return false;
        }

        var mimeapps = File.ReadAllText(mimeappsPath);

        // Failsafe in case I'm stupid beyond any legally allowed levels
        if (!File.Exists(mimeappsPathBackup))
        {
            logger.Warning("Created a backup for ./config/mimeapps.list under ./config/mimeapps.list.atlyssbackup!");
            File.WriteAllText(mimeappsPathBackup, mimeapps);
        }

        if (mimeapps.Contains(mimeappEntry))
        {
            logger.Info("App is already registered under ./config/mimeapps.list.");
            return true;
        }

        const string DefaultSectionName = "[Default Applications]";

        var indexOfSection = mimeapps.IndexOf(DefaultSectionName);

        if (indexOfSection == -1)
        {
            // Let's try to add this section if it's not there
            mimeapps += $"\n\n[Default Applications]\n{mimeappEntry}\n";
            logger.Info("Added default section and entry under ./config/mimeapps.list.");
        }
        else
        {
            // Let's try to update the default section
            mimeapps = mimeapps.Insert(indexOfSection + DefaultSectionName.Length, $"\n{mimeappEntry}");
            logger.Info("Updated default section with entry under ./config/mimeapps.list.");
        }

        File.WriteAllText(mimeappsPath, mimeapps);
        return true;
    }
}