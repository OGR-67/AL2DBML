using System;
using Microsoft.Extensions.DependencyInjection;

namespace AL2DBML.DI;

public static class AL2DbmlServiceExtensions
{
    public static IServiceCollection AddAL2Dbml(this IServiceCollection services)
        => services
            .AddApplication()
            .AddParser()
            .AddWriter();
}
