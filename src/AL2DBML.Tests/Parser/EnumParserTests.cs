using System.Reflection;
using AL2DBML.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AL2DBML.Tests.Parser;

public class EnumParserTests : TestBase
{
    private readonly IAlParser _parser;

    public EnumParserTests()
    {
        _parser = Services.GetRequiredService<IAlParser>();
    }

    [Fact]
    public void Parse_SimpleEnum_ReturnsCorrectModel()
    {
        var al = LoadFixture("Enums/CustomerStatus.al");

        var result = _parser.ParseEnum(al);

        Assert.Equal("Customer Status", result.Name);
        Assert.Equal(4, result.Values.Count);
        Assert.Contains(result.Values, v => v == "Active");
    }

    private static string LoadFixture(string path)
    {
        // Embed the fixture files so it's part of the assembly and can be loaded easily
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"AL2DBML.Tests.Fixtures.{path.Replace("/", ".")}";
        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
