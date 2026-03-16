using System;
using Microsoft.Extensions.DependencyInjection;

namespace AL2DBML.DI;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
        => services;
}
