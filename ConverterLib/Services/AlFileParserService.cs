using System.Text.RegularExpressions;
using ConverterLib.Models;

namespace ConverterLib.Services;

public class AlFileParserService
{
    private readonly string _fileContent;
    private readonly string _filePath;

    public AlFileParserService(string filePath)
    {
        _filePath = filePath;
        _fileContent = File.ReadAllText(filePath)
            .Replace("\r\n", "\n")
            .Replace("\r", "\n");

        // Remove comments and pragmas
        _fileContent = Regex.Replace(_fileContent,
            @"//.*?$|/\*.*?\*/|#pragma\s+.*?$",
            string.Empty,
            RegexOptions.Singleline | RegexOptions.Multiline);
    }

    public void ParseFile(ref OutputSchema outputSchema)
    {
        if (IsEnumFile())
        {
            ParseEnumFile(ref outputSchema);
        }
        else if (IsEnumExtensionFile())
        {
            ParseEnumExtensionFile(ref outputSchema);
        }
        else if (IsTableFile())
        {
            ParseTableFile(ref outputSchema);
        }
        else if (IsTableExtensionFile())
        {
            ParseTableExtensionFile(ref outputSchema);
        }
        else
        {
            throw new Exception($"Unsupported AL file type: {_filePath}");
        }
    }

    private bool IsEnumFile() =>
        Regex.IsMatch(_fileContent, @"^\s*enum\s+\d+\s+(""[^""]+""|\w+)",
            RegexOptions.IgnoreCase | RegexOptions.Multiline);

    private bool IsEnumExtensionFile() =>
        Regex.IsMatch(_fileContent, @"^\s*enumextension\s+\d+\s+(""[^""]+""|\w+)\s+extends\s+(""[^""]+""|\w+)\s+{",
            RegexOptions.Multiline);

    private bool IsTableFile() =>
        Regex.IsMatch(_fileContent, @"^\s*table\s+\d+\s+(""[^""]+""|\w+)",
            RegexOptions.Multiline);

    private bool IsTableExtensionFile() =>
        Regex.IsMatch(_fileContent, @"^\s*tableextension\s+\d+\s+(""[^""]+""|\w+)\s+extends\s+(""[^""]+""|\w+)\s+{",
            RegexOptions.Multiline);

    private void ParseTableFile(ref OutputSchema outputSchema)
    {
        var tableName = ExtractTableName(@"^\s*table\s+\d+\s+(""[^""]+""|\w+)\s*{");
        var table = GetOrCreateTable(tableName, ref outputSchema, out bool isNewTable);

        if (isNewTable) outputSchema.Tables.Add(table);

        var primaryKeys = GetPrimaryKeys();
        ParseFields(_fileContent, table, ref outputSchema, primaryKeys);
    }

    private void ParseTableExtensionFile(ref OutputSchema outputSchema)
    {
        var tableName = ExtractTableName(@"^\s*tableextension\s+\d+\s+(""[^""]+""|\w+)\s+extends\s+(""[^""]+""|\w+)\s*{", 2);
        var table = GetOrCreateTable(tableName, ref outputSchema, out bool isNewTable);

        if (isNewTable) outputSchema.Tables.Add(table);

        ParseFields(_fileContent, table, ref outputSchema, new List<string>());
    }

    private void ParseFields(string content, DBMLTable table, ref OutputSchema outputSchema, List<string> primaryKeys)
    {
        var fieldMatches = Regex.Matches(content,
            @"^\s*field\s*\(\s*(\d+);\s*(""[^""]+""|\w+)\s*;\s*(Enum)?\s*(""[^""]+""|\w+(\[\d+\])?)\s*\)\s*{([^{}]*)}",
            RegexOptions.Multiline);

        foreach (Match fieldMatch in fieldMatches)
        {
            var columnName = CleanName(fieldMatch.Groups[2].Value);
            var columnType = CleanName(fieldMatch.Groups[4].Value);
            var fieldBody = fieldMatch.Groups[6].Value;

            ProcessFieldBody(fieldBody, columnType, ref outputSchema,
                out string referenceTable,
                out string referenceField,
                out bool isFlowField,
                out string calcFormula);

            var existingColumn = table.Columns.FirstOrDefault(c => c.Name == columnName);
            if (existingColumn == null)
            {
                table.Columns.Add(new DBMLColumn
                {
                    Name = columnName,
                    Type = columnType,
                    IsPrimaryKey = primaryKeys.Contains(columnName),
                    IsFlowfield = isFlowField,
                    CalcFormula = calcFormula,
                    References = [referenceTable, referenceField]
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

    private string ExtractTableName(string pattern, int groupIndex = 1)
    {
        var match = Regex.Match(_fileContent, pattern, RegexOptions.Multiline);
        return match.Success
            ? CleanName(match.Groups[groupIndex].Value)
            : throw new Exception($"Table name not found in file: {_filePath}");
    }

    private List<string> GetPrimaryKeys()
    {
        var keyMatch = Regex.Match(_fileContent,
            @"^\s*key\s*\(\s*(""[^""]+""|\w+)\s*;\s*([^)]*)",
            RegexOptions.Multiline);

        return keyMatch.Success
            ? keyMatch.Groups[2].Value.Split(',')
                .Select(k => CleanName(k.Trim()))
                .ToList()
            : new List<string>();
    }

    private void ProcessFieldBody(string fieldBody, string columnType, ref OutputSchema outputSchema,
        out string referenceTable, out string referenceField, out bool isFlowField, out string calcFormula)
    {
        referenceTable = referenceField = calcFormula = string.Empty;
        isFlowField = false;

        // Check if flowfield
        var flowFieldMatch = Regex.Match(fieldBody, @"^\s*FieldClass\s*=\s*FlowField\s*;", RegexOptions.Multiline);
        if (flowFieldMatch.Success)
        {
            isFlowField = true;
            var calcFormulaMatch = Regex.Match(fieldBody, @"^\s*CalcFormula\s*=\s*([^;]+)\s*;", RegexOptions.Multiline);
            if (calcFormulaMatch.Success)
                calcFormula = Regex.Replace(calcFormulaMatch.Groups[1].Value.Trim(), @"\s+", " "); // Remove line breaks and extra spaces
        }

        // Check table relations
        var relationMatch = Regex.Match(fieldBody,
            @"^\s*TableRelation\s*=\s*(""[^""]+""|\w+)(\.(""[^""]+""|\w+))?",
            RegexOptions.Multiline);

        if (relationMatch.Success)
        {
            referenceTable = CleanName(relationMatch.Groups[1].Value);
            referenceField = relationMatch.Groups.Count > 3 && relationMatch.Groups[3].Success
                ? CleanName(relationMatch.Groups[3].Value)
                : "UnknownField";

            EnsureReferenceExists(referenceTable, referenceField, columnType, ref outputSchema);
        }
    }

    private void EnsureReferenceExists(string tableName, string fieldName, string fieldType, ref OutputSchema outputSchema)
    {
        var refTable = GetOrCreateTable(tableName, ref outputSchema, out bool isNew);
        var targetField = !string.IsNullOrEmpty(fieldName) ? fieldName : "Id";

        if (refTable.Columns.All(c => c.Name != targetField))
        {
            refTable.Columns.Add(new DBMLColumn { Name = targetField, Type = fieldType });
        }

        if (isNew) outputSchema.Tables.Add(refTable);
    }

    private DBMLTable GetOrCreateTable(string name, ref OutputSchema outputSchema, out bool isNew)
    {
        var table = outputSchema.Tables.FirstOrDefault(t => t.Name == name);
        isNew = table == null;
        return table ?? new DBMLTable { Name = name };
    }

    private void ParseEnumFile(ref OutputSchema outputSchema)
    {
        // Take into account that enum can implement interfaces
        // var enumName = ExtractEnumName(@"^\s*enum\s+\d+\s+(""[^""]+""|\w+)\s*{");
        var enumName = ExtractEnumName(@"^\s*enum\s+\d+\s+(""[^""]+""|\w+)(\s+implements\s+(""[^""]+""|\w+)(\s*,\s*(""[^""]+""|\w+))*)?\s*{", 1);
        var dbmlEnum = GetOrCreateEnum(enumName, ref outputSchema, out bool isNew);
        dbmlEnum.Values = GetEnumValues();
        if (isNew) outputSchema.Enums.Add(dbmlEnum);
    }

    private void ParseEnumExtensionFile(ref OutputSchema outputSchema)
    {
        var enumName = ExtractEnumName(@"^\s*enumextension\s+\d+\s+(""[^""]+""|\w+)\s+extends\s+(""[^""]+""|\w+)\s*{", 2);
        var dbmlEnum = GetOrCreateEnum(enumName, ref outputSchema, out bool isNew);
        var newValues = GetEnumValues();

        foreach (var value in newValues)
        {
            if (!dbmlEnum.Values.Contains(value)) dbmlEnum.Values.Add(value);
        }

        if (isNew) outputSchema.Enums.Add(dbmlEnum);
    }

    private string ExtractEnumName(string pattern, int groupIndex = 1)
    {
        var match = Regex.Match(_fileContent, pattern, RegexOptions.Multiline);
        return match.Success
            ? CleanName(match.Groups[groupIndex].Value)
            : throw new Exception($"Enum name not found in file: {_filePath}");
    }

    private List<string> GetEnumValues() =>
        Regex.Matches(_fileContent, @"^\s*value\(\s*\d+;\s*(""[^""]+""|\w+)", RegexOptions.Multiline)
            .Select(m => CleanName(m.Groups[1].Value))
            .ToList();

    private DBMLEnum GetOrCreateEnum(string name, ref OutputSchema outputSchema, out bool isNew)
    {
        var dbmlEnum = outputSchema.Enums.FirstOrDefault(e => e.Name == name);
        isNew = dbmlEnum == null;
        return dbmlEnum ?? new DBMLEnum { Name = name, Values = new List<string>() };
    }

    private static string CleanName(string name)
    {
        if (name.Contains('/'))
        {
            if (!name.StartsWith('"'))
                return $"\"{name}\"";
        }
        return name.Replace("\"", "");
    }
}
