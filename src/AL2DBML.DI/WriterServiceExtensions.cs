using AL2DBML.Application.Interfaces;
using AL2DBML.DBMLWriter;
using Microsoft.Extensions.DependencyInjection;

namespace AL2DBML.DI;

public static class WriterServiceExtensions
{
    public static IServiceCollection AddWriter(this IServiceCollection services)
    {
        services.AddScoped<ISchemaPostProcessor, SchemaPostProcessor>();
        services.AddScoped<IDBMLWriter, DBLMWriter>();
        return services;
    }
}
