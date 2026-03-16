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
    public void DetectFileType_Enum_ReturnsEnum()
    {
        var al = LoadFixture("Enums/CustomerStatus.al");

        var result = _parser.DetectFileType(al);

        Assert.Equal(Core.Enums.AlFileType.Enum, result);
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

    [Fact]
    public void Parse_EnumExtension_ReturnsCorrectModel()
    {
        var al = LoadFixture("EnumExts/CustomerStatusExtension.al");

        var result = _parser.ParseEnumExtension(al);

        Assert.Equal("Customer Status", result.Name);
        Assert.Single(result.Values, v => v == "Prospect");
    }

    [Fact]
    public void Parse_EnumExtension_MergesWithExistingEnum()
    {
        var enumAl = LoadFixture("Enums/CustomerStatus.al");
        var extAl = LoadFixture("EnumExts/CustomerStatusExtension.al");

        var enumResult = _parser.ParseEnum(enumAl);
        var extResult = _parser.ParseEnumExtension(extAl);

        Assert.Same(enumResult, extResult); // Should be the same instance
        Assert.Equal(5, enumResult.Values.Count); // Original 4 + 1 from extension
        Assert.Contains(enumResult.Values, v => v == "Prospect");
    }

    [Fact]
    public void Parse_Enum_ComplicateName()
    {
        var al = LoadFixture("Enums/ComplicateName.al");

        var result = _parser.ParseEnum(al);

        Assert.Equal("Complicate / Name", result.Name);
        Assert.Equal(2, result.Values.Count);
        Assert.Contains(result.Values, v => v == "True/False");
    }

    [Fact]
    public void Parse_Enum_NoQuotesInName()
    {
        var al = LoadFixture("Enums/NoQuote.al");

        var result = _parser.ParseEnum(al);

        Assert.Equal("NoQuote", result.Name);
        Assert.Equal(2, result.Values.Count);
        Assert.Contains(result.Values, v => v == "Quote");
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
