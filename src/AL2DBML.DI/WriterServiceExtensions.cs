using AL2DBML.Application.Interfaces;
using DBMLWriter;
using Microsoft.Extensions.DependencyInjection;

namespace AL2DBML.DI;

public static class WriterServiceExtensions
{
    public static IServiceCollection AddWriter(this IServiceCollection services)
    {
        services.AddScoped<ISchemaPostProcessor, SchemaPostProcessor>();
        services.AddScoped<IDBMLWriter, Writer>();
        return services;
    }
}
