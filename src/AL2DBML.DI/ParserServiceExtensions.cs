using AL2DBML.Application.Interfaces;
using AL2DBML.Parser;
using Microsoft.Extensions.DependencyInjection;

namespace AL2DBML.DI;

public static class ParserServiceExtensions
{
    public static IServiceCollection AddParser(this IServiceCollection services)
    {
        // Singleton because we want to maintain state across multiple parsing operations
        services.AddSingleton<IAlParser, AlParser>();
        return services;
    }
}
