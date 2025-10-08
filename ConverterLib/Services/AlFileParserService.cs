using System.Text.RegularExpressions;
using ConverterLib.Models;

namespace ConverterLib.Services;

public class AlFileParserService
{
    private string _fileContent;
    private string _filePath;

    public AlFileParserService(string filePath)
    {
        _filePath = filePath;
        _fileContent = File.ReadAllText(filePath);
        // Normalize line endings to \n
        _fileContent = _fileContent.Replace("\r\n", "\n").Replace("\r", "\n");
        // Remove comments
        _fileContent = Regex.Replace(_fileContent, @"//.*?$|/\*.*?\*/", string.Empty, RegexOptions.Singleline | RegexOptions.Multiline);
        // Remove pragmas
        _fileContent = Regex.Replace(_fileContent, @"#pragma\s+.*?$", string.Empty, RegexOptions.Multiline);
    }

    private bool IsEnumFile()
    {
        var regex = new Regex(@"^\s*enum\s+\d+\s+(""[^""]+""|\w+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        return regex.IsMatch(_fileContent);
    }
    private bool IsEnumExtensionFile()
    {
        var RegEx = new Regex(@"^enumextension\s+\d+\s+(""[^""]+""|\w+)\s+extends\s+(""[^""]+""|\w+)\s+{", RegexOptions.Multiline);
        return RegEx.IsMatch(_fileContent);
    }

    private bool IsTableFile()
    {
        var RegEx = new Regex(@"^\s*table\s+\d+\s+(""[^""]+""|\w+)", RegexOptions.Multiline);
        return RegEx.IsMatch(_fileContent);
    }

    private bool IsTableExtensionFile()
    {
        var RegEx = new Regex(@"^tableextension\s+\d+\s+(""[^""]+""|\w+)\s+extends\s+(""[^""]+""|\w+)\s+{", RegexOptions.Multiline);
        return RegEx.IsMatch(_fileContent);
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

    private void ParseTableFile(ref OutputSchema outputSchema)
    {
        var tableNameMatch = Regex.Match(_fileContent, @"^\s*table\s+\d+\s+(""[^""]+""|\w+)\s*{", RegexOptions.Multiline);
        if (!tableNameMatch.Success)
        {
            throw new Exception($"Table name not found in file: {_filePath}");
        }
        var tableName = tableNameMatch.Groups[1].Value;
        tableName = CleanName(tableName);

        var isNewTable = false;
        var table = GetTable(tableName, ref outputSchema, ref isNewTable);

        var primaryKeys = GetPrimaryKeys();
        // Example field header definitions:
        // field(6; "Quantity Per Unit Of Measure"; Decimal)
        // field(7; "Unit Of Measure"; Code[10])
        // field(5; "AM / PM"; Enum "OC AM/PM")
        var columnMatches = Regex.Matches(_fileContent, @"^\s*field\s*\(\s*\d+;\s*(""[^""]+""|\w+)\s*;\s*(Enum)?\s*(""[^""]+""|\w+(\[\d+\])?)\s*\)\s*{([^{}]*)}", RegexOptions.Multiline);
        foreach (Match columnMatch in columnMatches)
        {
            var columnName = CleanName(columnMatch.Groups[1].Value);
            var columnType = CleanName(columnMatch.Groups[3].Value);
            var fieldBodyStr = columnMatch.Groups[5].Value;

            // Extract additional properties from field body if needed
            ProcessFieldBody(fieldBodyStr, columnType, ref outputSchema, out string ReferenceTable, out string ReferenceField, out bool IsFlowfield, out string CalcFormula);

            // Check if the column already exists
            // If it does, update its properties
            // If it doesn't, add it

            if (!table.Columns.Any(c => c.Name == columnName))
            {
                var column = new DBMLColumn
                {
                    Name = columnName,
                    Type = columnType,
                    IsPrimaryKey = primaryKeys.Contains(columnName),
                    IsFlowfield = IsFlowfield,
                    CalcFormula = CalcFormula,
                    References = [ReferenceTable, ReferenceField]
                };

                table.Columns.Add(column);
            }
            else
            {
                var existingColumn = table.Columns.First(c => c.Name == columnName);
                existingColumn.Type = columnType; // Update type if needed
                existingColumn.IsPrimaryKey = primaryKeys.Contains(columnName); // Update primary key status
                existingColumn.IsFlowfield = IsFlowfield;
                existingColumn.CalcFormula = CalcFormula;
            }

            if (isNewTable)
            {
                outputSchema.Tables.Add(table);
                isNewTable = false; // Ensure we only add it once
            }
        }
    }

    private List<string> GetPrimaryKeys()
    {
        var keysStr = Regex.Match(_fileContent, @"^\s*key\s*\(\s*(""[^""]+""|\w+)\s*;\s*([^)]*)", RegexOptions.Multiline);
        if (!keysStr.Success)
            return new List<string>();
        return keysStr.Groups[2].Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(k => CleanName(k.Trim())).ToList();
    }

    private void ProcessFieldBody(string fieldBodyStr, string columnType, ref OutputSchema outputSchema, out string referenceTable, out string referenceField, out bool IsFlowfield, out string calcFormula)
    {
        referenceTable = string.Empty;
        referenceField = string.Empty;
        IsFlowfield = false;
        calcFormula = string.Empty;

        var flowfieldRegex = new Regex(@"^\s*FieldClass\s*=\s*(FlowField);", RegexOptions.Multiline);
        if (flowfieldRegex.IsMatch(fieldBodyStr))
        {
            IsFlowfield = true;
        }

        // ^\s*CalcFormula\s*=\s*([^;]*);
        if (IsFlowfield)
        {
            var CalcFormulaRegex = new Regex(@"^\s*CalcFormula\s*=\s*([^;]*);", RegexOptions.Multiline);
            if (CalcFormulaRegex.IsMatch(fieldBodyStr))
            {
                calcFormula = CalcFormulaRegex.Match(fieldBodyStr).Groups[1].Value.Trim();
            }
        }

        // ^\s*TableRelation\s*=\s*("[^"]+"|\w+)(\.("[^"]+"|\w+))?
        var tableRelationRegex = new Regex(@"^\s*TableRelation\s*=\s*(""[^""]+""|\w+)(\.(""[^""]+""|\w+))?", RegexOptions.Multiline);
        if (tableRelationRegex.IsMatch(fieldBodyStr))
        {
            var match = tableRelationRegex.Match(fieldBodyStr);
            referenceTable = CleanName(match.Groups[1].Value);
            if (match.Groups.Count > 3 && match.Groups[3].Success)
            {
                referenceField = CleanName(match.Groups[3].Value);
            }
            // Ensure the referenced table AND field exists in the schema
            var isNewTable = false;
            var refTable = GetTable(referenceTable, ref outputSchema, ref isNewTable);

            var fieldName = referenceField ?? "UnknownPrimaryKey";

            // Add the referenced field if it doesn't exist
            if (!refTable.Columns.Any(c => c.Name == fieldName))
            {
                refTable.Columns.Add(new DBMLColumn
                {
                    Name = fieldName,
                    Type = columnType,  // Use the same type as the referencing column
                });
            }

            if (isNewTable)
                outputSchema.Tables.Add(refTable);
        }
    }

    private void ParseTableExtensionFile(ref OutputSchema outputSchema)
    {
        var tableNameMatch = Regex.Match(_fileContent, @"\s*tableextension\s+\d+\s+(""[^""]+""|\w+)\s+extends\s+(""[^""]+""|\w+)\s+{", RegexOptions.Multiline);
        if (!tableNameMatch.Success)
        {
            throw new Exception($"Table extension name not found in file: {_filePath}");
        }
        var tableName = tableNameMatch.Groups[2].Value;
        tableName = CleanName(tableName);

        var isNewTable = false;
        var table = GetTable(tableName, ref outputSchema, ref isNewTable);

        if (isNewTable)
            outputSchema.Tables.Add(table);
    }

    private DBMLTable GetTable(string tableName, ref OutputSchema outputSchema, ref bool isNewTable)
    {
        var table = outputSchema.Tables.FirstOrDefault(t => t.Name == tableName);
        if (table == null)
        {
            table = new DBMLTable { Name = tableName };
            isNewTable = true;
        }
        return table;
    }

    private void ParseEnumFile(ref OutputSchema outputSchema)
    {
        var enumNameMatch = Regex.Match(_fileContent, @"^\s*enum\s+\d+\s+(""[^""]+""|\w+)\s*{", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        if (!enumNameMatch.Success)
        {
            throw new Exception($"Enum name not found in file: {_filePath}");
        }
        var enumName = enumNameMatch.Groups[1].Value;
        enumName = CleanName(enumName);

        var isNewEnum = false;
        var dbmlEnum = GetEnum(enumName, ref outputSchema, ref isNewEnum);

        var enumValues = GetEnumValues();

        foreach (var value in enumValues)
        {
            if (!dbmlEnum.Values.Contains(value))
            {
                dbmlEnum.Values.Add(value);
            }
        }

        if (isNewEnum)
            outputSchema.Enums.Add(dbmlEnum);
    }

    private void ParseEnumExtensionFile(ref OutputSchema outputSchema)
    {
        var enumNameMatch = Regex.Match(_fileContent, @"\s*enumextension\s+\d+\s+(""[^""]+""|\w+)\s+extends\s+(""[^""]+""|\w+)\s+{", RegexOptions.Multiline);
        if (!enumNameMatch.Success)
        {
            throw new Exception($"Enum extension name not found in file: {_filePath}");
        }
        var enumName = enumNameMatch.Groups[2].Value;
        enumName = CleanName(enumName);

        var isNewEnum = false;
        var dbmlEnum = GetEnum(enumName, ref outputSchema, ref isNewEnum);
        var enumValues = GetEnumValues();

        foreach (var value in enumValues)
        {
            if (!dbmlEnum.Values.Contains(value))
            {
                dbmlEnum.Values.Add(value);
            }
        }

        if (isNewEnum)
            outputSchema.Enums.Add(dbmlEnum);
    }

    private DBMLEnum GetEnum(string enumName, ref OutputSchema outputSchema, ref bool isNewEnum)
    {
        var dbmlEnum = outputSchema.Enums.FirstOrDefault(e => e.Name == enumName);
        if (dbmlEnum == null)
        {
            dbmlEnum = new DBMLEnum { Name = enumName };
            isNewEnum = true;
        }
        return dbmlEnum;
    }

    private List<string> GetEnumValues()
    {
        var enumValues = new List<string>();
        var matches = Regex.Matches(_fileContent, @"^\s*value\(\s*(\d+)\s*;\s*(""[^""]+""|\w+)", RegexOptions.Multiline);
        foreach (Match match in matches)
        {
            var value = match.Groups[2].Value;
            value = CleanName(value);
            enumValues.Add(value);
        }
        return enumValues;
    }

    private string CleanName(string name)
    {
        return name.Replace("\"", "");
    }
}
