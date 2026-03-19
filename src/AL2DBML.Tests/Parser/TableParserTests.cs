namespace AL2DBML.Tests.Parser;

public class TableParserTests : TestBase
{
    [Fact]
    public void DetectFileType_Table_ReturnsTable()
    {
        var al = LoadFixture("Tables/Customer.al");

        var result = _parser.DetectFileType(al);

        Assert.Equal(Core.Enums.AlFileType.Table, result);
    }

    [Fact]
    public void DetectFileType_TableWithSlashInName_ReturnsTable()
    {
        var al = LoadFixture("Tables/SalespersonPurchaser.al");

        var result = _parser.DetectFileType(al);

        Assert.Equal(Core.Enums.AlFileType.Table, result);
    }

    [Fact]
    public void DetectFileType_TableExtensionWithSlashInName_ReturnsTableExtension()
    {
        var al = LoadFixture("TableExts/SalespersonPurchaserExtension.al");

        var result = _parser.DetectFileType(al);

        Assert.Equal(Core.Enums.AlFileType.TableExtension, result);
    }

    [Fact]
    public void Parse_SimpleTable_ReturnsCorrectModel()
    {
        ResetParser();
        var al = LoadFixture("Tables/Customer.al");

        var result = _parser.ParseTable(al);

        Assert.Equal("Customer", result.Name);
        Assert.Equal(19, result.Fields.Count);
        Assert.Contains(result.Fields, f => f.Name == "No.");
        Assert.Contains(result.Fields, (f) => f.Name == "No." && f.IsPrimaryKey == true);
        Assert.Contains(result.Fields, f => f.Name == "Search Name" && f.Type == "Code[100]" && f.IsPrimaryKey == false);
    }

    [Fact]
    public void Parse_TableExtension_ReturnsCorrectModel()
    {
        ResetParser();
        var al = LoadFixture("TableExts/CustomerExtension.al");

        var result = _parser.ParseTableExtension(al);

        Assert.Equal("Customer", result.Name);
        Assert.Equal(4, result.Fields.Count);
    }

    [Fact]
    public void Parse_TableExtension_ExtendsExistingTable()
    {
        ResetParser();
        var baseTable = _parser.ParseTable(LoadFixture("Tables/Customer.al"));
        var extension = _parser.ParseTableExtension(LoadFixture("TableExts/CustomerExtension.al"));

        Assert.Same(baseTable, extension); // Should be the same instance
        Assert.Equal(23, baseTable.Fields.Count); // Original 19 + 4 from
    }

    [Fact]
    public void Parse_Table_WithSlashInName_ReturnsCorrectName()
    {
        ResetParser();
        var al = LoadFixture("Tables/SalespersonPurchaser.al");

        var result = _parser.ParseTable(al);

        Assert.Equal("Salesperson/Purchaser", result.Name);
        Assert.Equal(5, result.Fields.Count);
        Assert.Contains(result.Fields, f => f.Name == "Code" && f.IsPrimaryKey == true);
    }

    [Fact]
    public void Parse_Table_WithSlashInFieldName_ReturnsCorrectFieldName()
    {
        ResetParser();
        var al = LoadFixture("Tables/SalespersonPurchaser.al");

        var result = _parser.ParseTable(al);

        Assert.Contains(result.Fields, f => f.Name == "Salesperson/Purchaser Type");
    }

    [Fact]
    public void Parse_Table_WithEnumTypeField_ReturnsCorrectType()
    {
        ResetParser();
        var al = LoadFixture("Tables/SalespersonPurchaser.al");

        var result = _parser.ParseTable(al);

        Assert.Contains(result.Fields, f => f.Name == "Salesperson/Purchaser Type" && f.Type == "Salesperson/Purchaser Type");
    }

    [Fact]
    public void Parse_Table_WithCompositePrimaryKey_MarksBothFieldsAsPrimaryKey()
    {
        ResetParser();
        var al = LoadFixture("Tables/PurchaseHeader.al");

        var result = _parser.ParseTable(al);

        Assert.Equal(8, result.Fields.Count);
        Assert.Contains(result.Fields, f => f.Name == "Document Type" && f.IsPrimaryKey == true);
        Assert.Contains(result.Fields, f => f.Name == "No." && f.IsPrimaryKey == true);
        Assert.Contains(result.Fields, f => f.Name == "Amount" && f.IsPrimaryKey == false);
    }

    [Fact]
    public void Parse_Table_WithSlashInFieldName_PurchaseHeader()
    {
        ResetParser();
        var al = LoadFixture("Tables/PurchaseHeader.al");

        var result = _parser.ParseTable(al);

        Assert.Contains(result.Fields, f => f.Name == "Salesperson/Purchaser Code");
        Assert.Contains(result.Fields, f => f.Name == "Buy-from Vendor No.");
        Assert.Contains(result.Fields, f => f.Name == "Pay-to Vendor No.");
        Assert.Contains(result.Fields, f => f.Name == "Outstanding Amount (LCY)");
    }

    [Fact]
    public void Parse_Table_WithEnumTypeField_PurchaseHeader()
    {
        ResetParser();
        var al = LoadFixture("Tables/PurchaseHeader.al");

        var result = _parser.ParseTable(al);

        Assert.Contains(result.Fields, f => f.Name == "Document Type" && f.Type == "Purchase Document Type");
    }

    [Fact]
    public void Parse_Table_WithDotsInTableName_ReturnsCorrectName()
    {
        ResetParser();
        var al = LoadFixture("Tables/GenJournalLine.al");

        var result = _parser.ParseTable(al);

        Assert.Equal("Gen. Journal Line", result.Name);
        Assert.Equal(8, result.Fields.Count);
    }

    [Fact]
    public void Parse_Table_GenJournalLine_CompositePrimaryKey()
    {
        ResetParser();
        var al = LoadFixture("Tables/GenJournalLine.al");

        var result = _parser.ParseTable(al);

        Assert.Contains(result.Fields, f => f.Name == "Journal Template Name" && f.IsPrimaryKey == true);
        Assert.Contains(result.Fields, f => f.Name == "Line No." && f.IsPrimaryKey == true);
        Assert.Contains(result.Fields, f => f.Name == "Amount" && f.IsPrimaryKey == false);
    }

    [Fact]
    public void Parse_Table_GenJournalLine_MultipleEnumTypeFields()
    {
        ResetParser();
        var al = LoadFixture("Tables/GenJournalLine.al");

        var result = _parser.ParseTable(al);

        Assert.Contains(result.Fields, f => f.Name == "Account Type" && f.Type == "Gen. Journal Account Type");
        Assert.Contains(result.Fields, f => f.Name == "Document Type" && f.Type == "Gen. Journal Document Type");
    }

    [Fact]
    public void Parse_TableExtension_WithSlashInExtendedTableName_ReturnsCorrectTableName()
    {
        ResetParser();
        var al = LoadFixture("TableExts/SalespersonPurchaserExtension.al");

        var result = _parser.ParseTableExtension(al);

        Assert.Equal("Salesperson/Purchaser", result.Name);
        Assert.Equal(2, result.Fields.Count);
    }

    [Fact]
    public void Parse_TableExtension_ExtendsExistingTableWithSlashName()
    {
        ResetParser();
        var baseTable = _parser.ParseTable(LoadFixture("Tables/SalespersonPurchaser.al"));
        var extension = _parser.ParseTableExtension(LoadFixture("TableExts/SalespersonPurchaserExtension.al"));

        Assert.Same(baseTable, extension);
        Assert.Equal(7, baseTable.Fields.Count); // 5 original + 2 from extension
    }

    [Fact]
    public void Parse_TableExtension_WithDotsInExtendedTableName()
    {
        ResetParser();
        var baseTable = _parser.ParseTable(LoadFixture("Tables/GenJournalLine.al"));
        var extension = _parser.ParseTableExtension(LoadFixture("TableExts/GenJournalLineExtension.al"));

        Assert.Same(baseTable, extension);
        Assert.Equal(9, baseTable.Fields.Count); // 8 original + 1 from extension
    }

    [Fact]
    public void Parse_TableExtension_PurchaseHeader_AddsEnumField()
    {
        ResetParser();
        var baseTable = _parser.ParseTable(LoadFixture("Tables/PurchaseHeader.al"));
        var extension = _parser.ParseTableExtension(LoadFixture("TableExts/PurchaseHeaderExtension.al"));

        Assert.Same(baseTable, extension);
        Assert.Equal(10, baseTable.Fields.Count); // 8 original + 2 from extension
        Assert.Contains(baseTable.Fields, f => f.Name == "Approval Status" && f.Type == "Approval Status");
    }
}
