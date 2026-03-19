namespace AL2DBML.Tests.Parser;

public class EnumParserTests : TestBase
{
    [Fact]
    public void DetectFileType_Enum_ReturnsEnum()
    {
        var al = LoadFixture("Enums/CustomerStatus.al");

        var result = _parser.DetectFileType(al);

        Assert.Equal(Core.Enums.AlFileType.Enum, result);
    }

    [Fact]
    public void DetectFileType_EnumWithImplements_ReturnsEnum()
    {
        var al = LoadFixture("Enums/PaymentTermsCalcType.al");

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

    [Fact]
    public void Parse_Enum_WithSlashInName_ReturnsCorrectName()
    {
        var al = LoadFixture("Enums/SalespersonPurchaserType.al");

        var result = _parser.ParseEnum(al);

        Assert.Equal("Salesperson/Purchaser Type", result.Name);
        Assert.Equal(3, result.Values.Count);
        Assert.Contains(result.Values, v => v == "Internal");
        Assert.Contains(result.Values, v => v == "External");
    }

    [Fact]
    public void Parse_Enum_WithDotsInName_ReturnsCorrectName()
    {
        var al = LoadFixture("Enums/GenJournalDocumentType.al");

        var result = _parser.ParseEnum(al);

        Assert.Equal("Gen. Journal Document Type", result.Name);
        Assert.Equal(4, result.Values.Count);
        Assert.Contains(result.Values, v => v == "Payment");
        Assert.Contains(result.Values, v => v == "Credit Memo");
    }

    [Fact]
    public void Parse_Enum_WithSingleImplements_ExtractsNameOnly()
    {
        var al = LoadFixture("Enums/PaymentTermsCalcType.al");

        var result = _parser.ParseEnum(al);

        Assert.Equal("Payment Terms Calc. Type", result.Name);
        Assert.Equal(3, result.Values.Count);
        Assert.Contains(result.Values, v => v == "Fixed Day");
        Assert.Contains(result.Values, v => v == "Current Month");
    }

    [Fact]
    public void Parse_Enum_WithMultipleImplements_ExtractsNameOnly()
    {
        var al = LoadFixture("Enums/VATBusinessPostingGroup.al");

        var result = _parser.ParseEnum(al);

        Assert.Equal("VAT Business Posting Group", result.Name);
        Assert.Equal(4, result.Values.Count);
        Assert.Contains(result.Values, v => v == "Domestic");
        Assert.Contains(result.Values, v => v == "EU");
        Assert.Contains(result.Values, v => v == "Export");
    }

    [Fact]
    public void Parse_EnumExtension_WithSlashInExtendedEnumName_ReturnsCorrectName()
    {
        var al = LoadFixture("EnumExts/SalespersonPurchaserTypeExtension.al");

        var result = _parser.ParseEnumExtension(al);

        Assert.Equal("Salesperson/Purchaser Type", result.Name);
        Assert.Equal(2, result.Values.Count);
        Assert.Contains(result.Values, v => v == "Agency");
        Assert.Contains(result.Values, v => v == "Freelance");
    }

    [Fact]
    public void Parse_EnumExtension_WithDotsInExtendedEnumName_ReturnsCorrectName()
    {
        var al = LoadFixture("EnumExts/GenJournalDocumentTypeExtension.al");

        var result = _parser.ParseEnumExtension(al);

        Assert.Equal("Gen. Journal Document Type", result.Name);
        Assert.Single(result.Values, v => v == "Finance Charge Memo");
    }

    [Fact]
    public void Parse_EnumExtension_MergesWithSlashNameEnum()
    {
        var enumAl = LoadFixture("Enums/SalespersonPurchaserType.al");
        var extAl = LoadFixture("EnumExts/SalespersonPurchaserTypeExtension.al");

        var enumResult = _parser.ParseEnum(enumAl);
        var extResult = _parser.ParseEnumExtension(extAl);

        Assert.Same(enumResult, extResult);
        Assert.Equal(5, enumResult.Values.Count); // 3 original + 2 from extension
        Assert.Contains(enumResult.Values, v => v == "Agency");
        Assert.Contains(enumResult.Values, v => v == "Freelance");
    }

    [Fact]
    public void Parse_EnumExtension_MergesWithDotsNameEnum()
    {
        var enumAl = LoadFixture("Enums/GenJournalDocumentType.al");
        var extAl = LoadFixture("EnumExts/GenJournalDocumentTypeExtension.al");

        var enumResult = _parser.ParseEnum(enumAl);
        var extResult = _parser.ParseEnumExtension(extAl);

        Assert.Same(enumResult, extResult);
        Assert.Equal(5, enumResult.Values.Count); // 4 original + 1 from extension
        Assert.Contains(enumResult.Values, v => v == "Finance Charge Memo");
    }
}
