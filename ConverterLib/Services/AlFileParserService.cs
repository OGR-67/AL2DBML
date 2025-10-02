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
        // remove quotes if any
        tableName = tableName.Replace("\"", "");

        var table = GetTable(tableName, outputSchema);

        outputSchema.Tables.Add(table);
    }

    private void ParseTableExtensionFile(ref OutputSchema outputSchema)
    {
        var tableNameMatch = Regex.Match(_fileContent, @"\s*tableextension\s+\d+\s+(""[^""]+""|\w+)\s+extends\s+(""[^""]+""|\w+)\s+{", RegexOptions.Multiline);
        if (!tableNameMatch.Success)
        {
            throw new Exception($"Table extension name not found in file: {_filePath}");
        }
        var tableName = tableNameMatch.Groups[2].Value;
        tableName = tableName.Replace("\"", "");

        var table = GetTable(tableName, outputSchema);

        outputSchema.Tables.Add(table);
    }

    private DBMLTable GetTable(string tableName, OutputSchema outputSchema)
    {
        var table = outputSchema.Tables.FirstOrDefault(t => t.Name == tableName);
        if (table == null)
        {
            table = new DBMLTable { Name = tableName };
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
        enumName = enumName.Replace("\"", "");

        var dbmlEnum = GetEnum(enumName, outputSchema);

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

        var dbmlEnum = GetEnum(enumName, outputSchema);

        outputSchema.Enums.Add(dbmlEnum);
    }

    private DBMLEnum GetEnum(string enumName, OutputSchema outputSchema)
    {
        var dbmlEnum = outputSchema.Enums.FirstOrDefault(e => e.Name == enumName);
        if (dbmlEnum == null)
        {
            dbmlEnum = new DBMLEnum { Name = enumName };
        }
        return dbmlEnum;
    }
}
