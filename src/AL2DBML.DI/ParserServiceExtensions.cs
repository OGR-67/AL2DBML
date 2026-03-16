using AL2DBML.Application.Interfaces;
using AL2DBML.Parser;
using Microsoft.Extensions.DependencyInjection;

namespace AL2DBML.DI;

public static class ParserServiceExtensions
{
    public static IServiceCollection AddParser(this IServiceCollection services)
    {
        services.AddScoped<IAlParser, AlParser>();
        return services;
    }
}
