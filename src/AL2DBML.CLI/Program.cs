using AL2DBML.CLI.Commands;
using AL2DBML.DI;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;



// Register services
var services = new ServiceCollection();
services
    .AddAL2Dbml()
    .AddScoped<GenerateCommand>();

var registrar = new TypeRegistrar(services);

var app = new CommandApp(registrar);

app.Configure(config =>
{
    config.AddCommand<GenerateCommand>("generate");
});

return await app.RunAsync(args);

public sealed class TypeRegistrar(IServiceCollection services) : ITypeRegistrar
{
    public ITypeResolver Build() => new TypeResolver(services.BuildServiceProvider());

    public void Register(Type service, Type implementation) => services.AddSingleton(service, implementation);

    public void RegisterInstance(Type service, object implementation) => services.AddSingleton(service, implementation);

    public void RegisterLazy(Type service, Func<object> factory) => services.AddSingleton(service, _ => factory());
}

public sealed class TypeResolver(IServiceProvider provider) : ITypeResolver
{
    public object? Resolve(Type? type) => type == null ? null : provider.GetService(type);
}
