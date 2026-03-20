using System.Reflection;
using AL2DBML.Application.Interfaces;
using AL2DBML.DI;
using Microsoft.Extensions.DependencyInjection;

public abstract class TestBase
{
    protected IServiceProvider Services { get; }
    protected IAlParser _parser { get; private set; }
    protected IDBMLWriter _writer { get; private set; }

    protected TestBase()
    {
        Services = new ServiceCollection()
            .AddAL2Dbml()
            .BuildServiceProvider();

        _parser = Services.GetRequiredService<IAlParser>();
        _writer = Services.GetRequiredService<IDBMLWriter>();
    }

    protected void ResetParser()
    {
        _parser = Services.GetRequiredService<IAlParser>();
    }

    protected static string LoadFixture(string path)
    {
        // Embed the fixture files so it's part of the assembly and can be loaded easily
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"AL2DBML.Tests.Fixtures.{path.Replace("/", ".")}";
        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
