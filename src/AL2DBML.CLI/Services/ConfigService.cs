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

    public SharedConfig? LoadSharedConfig()
    {
        var path = Path.Combine(ConfigDir, SharedConfigFile);
        if (!File.Exists(path)) return null;
        return JsonSerializer.Deserialize<SharedConfig>(File.ReadAllText(path), JsonOptions);
    }

    public LocalConfig? LoadLocalConfig()
    {
        var path = Path.Combine(ConfigDir, LocalConfigFile);
        if (!File.Exists(path)) return null;
        return JsonSerializer.Deserialize<LocalConfig>(File.ReadAllText(path), JsonOptions);
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
