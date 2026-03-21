using AL2DBML.CLI.Models;

namespace AL2DBML.CLI.Services;

public interface IConfigService
{
    bool ConfigExists();
    SharedConfig? LoadSharedConfig();
    LocalConfig? LoadLocalConfig();
    void SaveSharedConfig(SharedConfig config);
    void SaveLocalConfig(LocalConfig config);
}
