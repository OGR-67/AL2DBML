using AL2DBML.Core.Models;

namespace AL2DBML.Tests.Writer;

public class WriterTests : TestBase
{

    // --- Schema vide ---

    [Fact]
    public async Task WriteDBMLAsync_EmptySchema_ReturnsEmptyString()
    {
        var result = await _writer.WriteDBMLAsync(new OutputSchema());

        Assert.Equal(string.Empty, result);
    }

    // --- Enums ---

    [Fact]
    public async Task WriteDBMLAsync_WithEnum_GeneratesEnumBlock()
    {
        var schema = new OutputSchema
        {
            Enums = [new DBMLEnum { Name = "Status", Values = ["Active", "Inactive"] }]
        };

        var result = await _writer.WriteDBMLAsync(schema);

        Assert.Contains("enum Status {", result);
        Assert.Contains("  Active", result);
        Assert.Contains("  Inactive", result);
    }

    [Fact]
    public async Task WriteDBMLAsync_WithEnumWithSpecialChars_QuotesIdentifiers()
    {
        var schema = new OutputSchema
        {
            Enums = [new DBMLEnum { Name = "My Enum", Values = ["Value 1"] }]
        };

        var result = await _writer.WriteDBMLAsync(schema);

        Assert.Contains("enum \"My Enum\" {", result);
        Assert.Contains("  \"Value 1\"", result);
    }

    // --- Tables ---

    [Fact]
    public async Task WriteDBMLAsync_WithSimpleTable_GeneratesTableBlock()
    {
        var schema = new OutputSchema
        {
            Tables = [new DBMLTable { Name = "Customer", Fields = [new DBMLColumn { Name = "Name", Type = "Code[20]" }] }]
        };

        var result = await _writer.WriteDBMLAsync(schema);

        Assert.Contains("table Customer {", result);
        Assert.Contains("  Name \"Code[20]\"", result);
    }

    [Fact]
    public async Task WriteDBMLAsync_WithPrimaryKeyField_AddsPkAttribute()
    {
        var schema = new OutputSchema
        {
            Tables =
            [
                new DBMLTable
                {
                    Name = "Customer",
                    Fields = [new DBMLColumn { Name = "No.", Type = "Code[20]", IsPrimaryKey = true }]
                }
            ]
        };

        var result = await _writer.WriteDBMLAsync(schema);

        Assert.Contains("  \"No.\" \"Code[20]\" [pk]", result);
    }

    [Fact]
    public async Task WriteDBMLAsync_WithReferenceField_AddsRefAttribute()
    {
        var schema = new OutputSchema
        {
            Tables =
            [
                new DBMLTable
                {
                    Name = "SalesLine",
                    Fields =
                    [
                        new DBMLColumn { Name = "CustomerNo", Type = "Code[20]", References = ["Customer", "Id"] }
                    ]
                }
            ]
        };

        var result = await _writer.WriteDBMLAsync(schema);

        Assert.Contains("ref: > Customer.Id", result);
    }

    [Fact]
    public async Task WriteDBMLAsync_WithReferenceToSpecialCharField_QuotesRef()
    {
        var schema = new OutputSchema
        {
            Tables =
            [
                new DBMLTable
                {
                    Name = "SalesLine",
                    Fields =
                    [
                        new DBMLColumn { Name = "CustomerNo", Type = "Code[20]", References = ["Customer", "No."] }
                    ]
                }
            ]
        };

        var result = await _writer.WriteDBMLAsync(schema);

        Assert.Contains("ref: > Customer.\"No.\"", result);
    }

    [Fact]
    public async Task WriteDBMLAsync_WithSpecialCharsInTableName_QuotesTableName()
    {
        var schema = new OutputSchema
        {
            Tables =
            [
                new DBMLTable
                {
                    Name = "Salesperson/Purchaser",
                    Fields = [new DBMLColumn { Name = "Code", Type = "Code[20]" }]
                }
            ]
        };

        var result = await _writer.WriteDBMLAsync(schema);

        Assert.Contains("table \"Salesperson/Purchaser\" {", result);
    }

    [Fact]
    public async Task WriteDBMLAsync_WithSpecialCharsInFieldName_QuotesFieldName()
    {
        var schema = new OutputSchema
        {
            Tables =
            [
                new DBMLTable
                {
                    Name = "Customer",
                    Fields = [new DBMLColumn { Name = "Search Name", Type = "Code[20]" }]
                }
            ]
        };

        var result = await _writer.WriteDBMLAsync(schema);

        Assert.Contains("  \"Search Name\" \"Code[20]\"", result);
    }

    [Fact]
    public async Task WriteDBMLAsync_WithFlowfield_AddsNoteAttribute()
    {
        var schema = new OutputSchema
        {
            Tables =
            [
                new DBMLTable
                {
                    Name = "Customer",
                    Fields =
                    [
                        new DBMLColumn
                        {
                            Name = "Balance",
                            Type = "Decimal",
                            IsFlowfield = true,
                            CalcFormula = "Sum(Entry.Amount)"
                        }
                    ]
                }
            ]
        };

        var result = await _writer.WriteDBMLAsync(schema);

        Assert.Contains("note: 'FlowField: CalcFormula = Sum(Entry.Amount)'", result);
    }

    [Fact]
    public async Task WriteDBMLAsync_WithFlowfieldWithSingleQuote_EscapesQuote()
    {
        var schema = new OutputSchema
        {
            Tables =
            [
                new DBMLTable
                {
                    Name = "Customer",
                    Fields =
                    [
                        new DBMLColumn
                        {
                            Name = "Balance",
                            Type = "Decimal",
                            IsFlowfield = true,
                            CalcFormula = "Sum('Entry'.Amount)"
                        }
                    ]
                }
            ]
        };

        var result = await _writer.WriteDBMLAsync(schema);

        Assert.Contains(@"CalcFormula = Sum(\'Entry\'.Amount)", result);
    }

    [Fact]
    public async Task WriteDBMLAsync_NonFlowfieldWithCalcFormula_DoesNotAddNote()
    {
        var schema = new OutputSchema
        {
            Tables =
            [
                new DBMLTable
                {
                    Name = "Customer",
                    Fields = [new DBMLColumn { Name = "Balance", Type = "Decimal", IsFlowfield = false, CalcFormula = "Sum(Entry.Amount)" }]
                }
            ]
        };

        var result = await _writer.WriteDBMLAsync(schema);

        Assert.DoesNotContain("note:", result);
    }

    [Fact]
    public async Task WriteDBMLAsync_EnumsBeforeTables()
    {
        var schema = new OutputSchema
        {
            Enums = [new DBMLEnum { Name = "Status", Values = ["Active"] }],
            Tables = [new DBMLTable { Name = "Customer", Fields = [new DBMLColumn { Name = "Id", Type = "Code[20]" }] }]
        };

        var result = await _writer.WriteDBMLAsync(schema);

        Assert.True(result.IndexOf("enum") < result.IndexOf("table"));
    }

    // --- Non-mutation de l'input ---

    [Fact]
    public async Task WriteDBMLAsync_DoesNotMutateInputSchema()
    {
        var schema = new OutputSchema
        {
            Tables =
            [
                new DBMLTable
                {
                    Name = "Customer",
                    Fields =
                    [
                        new DBMLColumn { Name = "Id", Type = "Code[20]", IsPrimaryKey = true },
                        new DBMLColumn { Name = "UnknownField", Type = "Code[20]" }
                    ]
                }
            ]
        };

        await _writer.WriteDBMLAsync(schema);

        Assert.Equal(2, schema.Tables[0].Fields.Count);
        Assert.Contains(schema.Tables[0].Fields, f => f.Name == "UnknownField");
    }

    // --- CleanupUnknownFieldReferences ---

    [Fact]
    public async Task WriteDBMLAsync_UnknownRef_ResolvedToSinglePkName()
    {
        var schema = new OutputSchema
        {
            Tables =
            [
                new DBMLTable
                {
                    Name = "Customer",
                    Fields = [new DBMLColumn { Name = "Id", Type = "Code[20]", IsPrimaryKey = true }]
                },
                new DBMLTable
                {
                    Name = "SalesLine",
                    Fields = [new DBMLColumn { Name = "CustomerNo", Type = "Code[20]", References = ["Customer", "UnknownField"] }]
                }
            ]
        };

        var result = await _writer.WriteDBMLAsync(schema);

        Assert.Contains("ref: > Customer.Id", result);
        Assert.DoesNotContain("UnknownField", result);
    }

    [Fact]
    public async Task WriteDBMLAsync_UnknownRef_AlignsTypeWithPkType()
    {
        var schema = new OutputSchema
        {
            Tables =
            [
                new DBMLTable
                {
                    Name = "Customer",
                    Fields = [new DBMLColumn { Name = "No", Type = "Code[20]", IsPrimaryKey = true }]
                },
                new DBMLTable
                {
                    Name = "SalesLine",
                    Fields = [new DBMLColumn { Name = "CustomerNo", Type = "Code[20]", References = ["Customer", "UnknownField"] }]
                }
            ]
        };

        var result = await _writer.WriteDBMLAsync(schema);

        Assert.Contains("CustomerNo \"Code[20]\"", result);
    }

    [Fact]
    public async Task WriteDBMLAsync_UnknownFieldColumn_RemovedFromTableWithSinglePk()
    {
        var schema = new OutputSchema
        {
            Tables =
            [
                new DBMLTable
                {
                    Name = "Customer",
                    Fields =
                    [
                        new DBMLColumn { Name = "Id", Type = "Code[20]", IsPrimaryKey = true },
                        new DBMLColumn { Name = "UnknownField", Type = "Code[20]" }
                    ]
                }
            ]
        };

        var result = await _writer.WriteDBMLAsync(schema);

        Assert.DoesNotContain("UnknownField", result);
    }

    [Fact]
    public async Task WriteDBMLAsync_UnknownRef_NotResolvedWhenTargetHasMultiplePks()
    {
        var schema = new OutputSchema
        {
            Tables =
            [
                new DBMLTable
                {
                    Name = "SalesLine",
                    Fields =
                    [
                        new DBMLColumn { Name = "DocType", Type = "Code[20]", IsPrimaryKey = true },
                        new DBMLColumn { Name = "LineNo", Type = "Code[20]", IsPrimaryKey = true }
                    ]
                },
                new DBMLTable
                {
                    Name = "SalesSubLine",
                    Fields = [new DBMLColumn { Name = "SalesNo", Type = "Code[20]", References = ["SalesLine", "UnknownField"] }]
                }
            ]
        };

        var result = await _writer.WriteDBMLAsync(schema);

        Assert.Contains("UnknownField", result);
    }

    // --- InferSinglePkWhenOnlyUnknownPlusOneColumn ---

    [Fact]
    public async Task WriteDBMLAsync_InfersPk_WhenSingleRealFieldPlusUnknownField()
    {
        var schema = new OutputSchema
        {
            Tables =
            [
                new DBMLTable
                {
                    Name = "Customer",
                    Fields =
                    [
                        new DBMLColumn { Name = "Id", Type = "Code[20]" },
                        new DBMLColumn { Name = "UnknownField", Type = "Code[20]" }
                    ]
                },
                new DBMLTable
                {
                    Name = "SalesLine",
                    Fields = [new DBMLColumn { Name = "CustomerNo", Type = "Code[20]", References = ["Customer", "UnknownField"] }]
                }
            ]
        };

        var result = await _writer.WriteDBMLAsync(schema);

        Assert.Contains("Id \"Code[20]\" [pk]", result);
        Assert.Contains("ref: > Customer.Id", result);
        Assert.DoesNotContain("UnknownField", result);
    }

    [Fact]
    public async Task WriteDBMLAsync_DoesNotInferPk_WhenMultipleRealFields()
    {
        var schema = new OutputSchema
        {
            Tables =
            [
                new DBMLTable
                {
                    Name = "Customer",
                    Fields =
                    [
                        new DBMLColumn { Name = "Id", Type = "Code[20]" },
                        new DBMLColumn { Name = "Name", Type = "Code[20]" },
                        new DBMLColumn { Name = "UnknownField", Type = "Code[20]" }
                    ]
                }
            ]
        };

        var result = await _writer.WriteDBMLAsync(schema);

        Assert.DoesNotContain("[pk]", result);
        Assert.Contains("UnknownField", result);
    }
}
