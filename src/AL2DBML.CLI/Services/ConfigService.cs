using System.Text.Json;
using AL2DBML.CLI.Models;

namespace AL2DBML.CLI.Services;

public class ConfigService : IConfigService
{
    private const string ConfigDir = ".al2dbml";
    private const string SharedConfigFile = "config.json";
    private const string LocalConfigFile = "config.local.json";

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public bool ConfigExists() => Directory.Exists(ConfigDir);

    public SharedConfig? LoadSharedConfig() => Load<SharedConfig>(Path.Combine(ConfigDir, SharedConfigFile));

    public LocalConfig? LoadLocalConfig() => Load<LocalConfig>(Path.Combine(ConfigDir, LocalConfigFile));

    private static T? Load<T>(string path)
    {
        if (!File.Exists(path)) return default;
        try
        {
            return JsonSerializer.Deserialize<T>(File.ReadAllText(path), JsonOptions);
        }
        catch (Exception e) when (e is IOException or JsonException)
        {
            return default;
        }
    }

    public void SaveSharedConfig(SharedConfig config)
    {
        Directory.CreateDirectory(ConfigDir);
        File.WriteAllText(Path.Combine(ConfigDir, SharedConfigFile), JsonSerializer.Serialize(config, JsonOptions));
    }

    public void SaveLocalConfig(LocalConfig config)
    {
        Directory.CreateDirectory(ConfigDir);
        File.WriteAllText(Path.Combine(ConfigDir, LocalConfigFile), JsonSerializer.Serialize(config, JsonOptions));
    }
}
