using AllinWeaponUnslotted.Helpers;
using AllinWeaponUnslotted.Interfaces;
using SPTarkov.DI.Annotations;
using System.Reflection;
using System.Text.Json;

namespace AllinWeaponUnslotted.Loaders;

[Injectable(InjectionType.Singleton)]
public class ConfigLoader
{
    public ConfigData Config { get; }

    public ConfigLoader(CustomLogger logger)
    {
        string modFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            ?? throw new InvalidOperationException("Unable to determine the mod directory.");
        string configDir = Path.Combine(modFolder, "config");
        string configPath = Path.Combine(configDir, "config.jsonc");
        string defaultConfigPath = Path.Combine(configDir, "defaultConfig.jsonc");

        try
        {
            // Check if config.jsonc exists
            if (!File.Exists(configPath))
            {
                if (File.Exists(defaultConfigPath))
                {
                    logger.Warning($"Config file not found. Copying defaultConfig.jsonc to config.jsonc...");
                    File.Copy(defaultConfigPath, configPath);
                }
                else
                {
                    logger.Error($"Neither config.jsonc nor defaultConfig.jsonc found in {configDir}. Using built-in defaults.");
                    Config = new ConfigData();
                    return;
                }
            }

            // Load config.jsonc
            var config = JsonSerializer.Deserialize<ConfigData>(
                File.ReadAllText(configPath),
                new JsonSerializerOptions
                {
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    PropertyNameCaseInsensitive = true
                });

            if (config == null)
            {
                logger.Error($"Config file is null. Loading default config.");
                Config = new ConfigData();
                return;
            }

            Config = config;
            //logger.LogWithColor($"[{GetType().Namespace}] Config loaded successfully.", LogTextColor.Green);
        }
        catch (Exception ex)
        {
            logger.Error($"Failed to load config: {ex.Message}");
            Config = new ConfigData();
        }
    }
}
