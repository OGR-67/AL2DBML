using System.Text.RegularExpressions;
using AL2DBML.Application.Interfaces;
using AL2DBML.Core.Enums;
using AL2DBML.Core.Models;
using AL2DBML.Parser.Helpers;

namespace AL2DBML.Parser;

public class AlParser : IAlParser
{
    private static class Patterns
    {
        public const string Enum = @"^\s*enum\s+\d+\s+(""[^""]+""|\w+)(\s+implements\s+(""[^""]+""|\w+)(\s*,\s*(""[^""]+""|\w+))*)?\s*{";
        public const string EnumExtension = @"^\s*enumextension\s+\d+\s+(""[^""]+""|\w+)\s+extends\s+(""[^""]+""|\w+)";
        public const string EnumValue = @"^\s*value\(\s*\d+;\s*(""[^""]+""|\w+)";
        public const string Table = @"^\s*table\s+\d+\s+(""[^""]+""|\w+)";
        public const string TableExtension = @"^\s*tableextension\s+\d+\s+(""[^""]+""|\w+)\s+extends\s+(""[^""]+""|\w+)";
        public const string Field = @"^\s*field\s*\(\s*(\d+);\s*(""[^""]+""|\w+)\s*;\s*(Enum)?\s*(""[^""]+""|\w+(\[\d+\])?)\s*\)\s*{([^{}]*)}";
        public const string Key = @"^\s*key\s*\(\s*(""[^""]+""|\w+)\s*;\s*([^)]*)\s*\)";
    }
    private readonly OutputSchema _outputSchema = new();

    public AlFileType DetectFileType(string content) => content switch
    {
        var c when Regex.IsMatch(c, Patterns.EnumExtension, RegexOptions.Multiline) => AlFileType.EnumExtension,
        var c when Regex.IsMatch(c, Patterns.Enum, RegexOptions.Multiline) => AlFileType.Enum,
        var c when Regex.IsMatch(c, Patterns.TableExtension, RegexOptions.Multiline) => AlFileType.TableExtension,
        var c when Regex.IsMatch(c, Patterns.Table, RegexOptions.Multiline) => AlFileType.Table,
        _ => AlFileType.Unknown
    };

    public DBMLEnum ParseEnum(string content)
    {
        var name = AlSyntaxHelper.ExtractMatch(content, Patterns.Enum);
        var dbmlEnum = GetOrCreateEnum(name, out bool isNew);
        dbmlEnum.Values = AlSyntaxHelper.ExtractAllMatches(content, Patterns.EnumValue);
        if (isNew) _outputSchema.Enums.Add(dbmlEnum);
        return dbmlEnum;
    }

    public DBMLEnum ParseEnumExtension(string content)
    {
        var enumName = AlSyntaxHelper.ExtractMatch(content, Patterns.EnumExtension, 2);
        var dbmlEnum = GetOrCreateEnum(enumName, out bool isNew);
        foreach (var value in AlSyntaxHelper.ExtractAllMatches(content, Patterns.EnumValue))
        {
            if (!dbmlEnum.Values.Contains(value)) dbmlEnum.Values.Add(value);
        }
        if (isNew) _outputSchema.Enums.Add(dbmlEnum);
        return dbmlEnum;
    }

    private DBMLEnum GetOrCreateEnum(string name, out bool isNew)
    {
        var existing = _outputSchema.Enums.FirstOrDefault(e => e.Name == name);
        isNew = existing is null;
        return existing ?? new DBMLEnum { Name = name, Values = [] };
    }

    public DBMLTable ParseTable(string alTableFileContent)
    {
        var name = AlSyntaxHelper.ExtractMatch(alTableFileContent, Patterns.Table);
        var dbmlTable = GetOrCreateTable(name, out bool isNew);

        var primaryKeys = GetPrimaryKeys(alTableFileContent);
        ParseFields(alTableFileContent, dbmlTable, primaryKeys);

        if (isNew) _outputSchema.Tables.Add(dbmlTable);
        return dbmlTable;
    }

    public DBMLTable ParseTableExtension(string alTableExtensionFileContent)
    {
        var tableName = AlSyntaxHelper.ExtractMatch(alTableExtensionFileContent, Patterns.TableExtension, 2);
        var dbmlTable = GetOrCreateTable(tableName, out bool isNew);

        var primaryKeys = GetPrimaryKeys(alTableExtensionFileContent);
        ParseFields(alTableExtensionFileContent, dbmlTable, primaryKeys);

        if (isNew) _outputSchema.Tables.Add(dbmlTable);
        return dbmlTable;
    }

    private List<string> GetPrimaryKeys(string content)
    {
        var keyMatches = Regex.Matches(content, Patterns.Key, RegexOptions.Multiline);

        foreach (Match keyMatch in keyMatches)
        {
            if (keyMatch.Success)
            {
                var keyName = AlSyntaxHelper.CleanName(keyMatch.Groups[1].Value);
                var keyFields = keyMatch.Groups[2].Value;
                // Check if this is the primary key by looking for "PK" or "Clustered = true" after it
                if (keyName.Equals("PK", StringComparison.OrdinalIgnoreCase) ||
                    content.Substring(keyMatch.Index).Contains("Clustered = true"))
                {
                    return keyFields.Split(',')
                        .Select(k => AlSyntaxHelper.CleanName(k.Trim()))
                        .ToList();
                }
            }
        }
        return [];
    }

    private void ParseFields(string content, DBMLTable table, List<string> primaryKeys)
    {
        var fieldMatches = Regex.Matches(content, Patterns.Field, RegexOptions.Multiline);

        foreach (Match fieldMatch in fieldMatches)
        {
            var columnName = AlSyntaxHelper.CleanName(fieldMatch.Groups[2].Value);
            var columnType = AlSyntaxHelper.CleanName(fieldMatch.Groups[4].Value);
            var fieldBody = fieldMatch.Groups[6].Value;

            var isFlowField = fieldBody.Contains("FieldClass") && fieldBody.Contains("FlowField");
            var calcFormula = ExtractCalcFormula(fieldBody);

            var existingColumn = table.Fields.FirstOrDefault(c => c.Name == columnName);
            if (existingColumn == null)
            {
                table.Fields.Add(new DBMLColumn
                {
                    Name = columnName,
                    Type = columnType,
                    IsPrimaryKey = primaryKeys.Contains(columnName),
                    IsFlowfield = isFlowField,
                    CalcFormula = calcFormula
                });
            }
            else
            {
                existingColumn.Type = columnType;
                existingColumn.IsPrimaryKey = primaryKeys.Contains(columnName);
                existingColumn.IsFlowfield = isFlowField;
                existingColumn.CalcFormula = calcFormula;
            }
        }
    }

    private string ExtractCalcFormula(string fieldBody)
    {
        var match = Regex.Match(fieldBody, @"CalcFormula\s*=\s*([^;]+)");
        return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
    }

    private DBMLTable GetOrCreateTable(string name, out bool isNew)
    {
        var existing = _outputSchema.Tables.FirstOrDefault(t => t.Name == name);
        isNew = existing is null;
        return existing ?? new DBMLTable { Name = name, Fields = [] };
    }

    public DBMLColumn ParseField(string alFieldContent)
    {
        var fieldMatch = Regex.Match(alFieldContent, Patterns.Field, RegexOptions.Multiline);

        if (!fieldMatch.Success)
            throw new FormatException("Field pattern not found or invalid");

        var columnName = AlSyntaxHelper.CleanName(fieldMatch.Groups[2].Value);
        var columnType = AlSyntaxHelper.CleanName(fieldMatch.Groups[4].Value);
        var fieldBody = fieldMatch.Groups[6].Value;

        var isFlowField = fieldBody.Contains("FieldClass") && fieldBody.Contains("FlowField");
        var calcFormula = ExtractCalcFormula(fieldBody);

        return new DBMLColumn
        {
            Name = columnName,
            Type = columnType,
            IsFlowfield = isFlowField,
            CalcFormula = calcFormula
        };
    }

    public OutputSchema GetOutputSchema()
    {
        return _outputSchema;
    }
}
