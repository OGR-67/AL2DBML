using System.Text.RegularExpressions;
using AL2DBML.Application.Interfaces;
using AL2DBML.Core.Enums;
using AL2DBML.Core.Models;

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

    private DBMLEnum GetOrCreateEnum(string name, out bool isNew)
    {
        var existing = _outputSchema.Enums.FirstOrDefault(e => e.Name == name);
        isNew = existing is null;
        return existing ?? new DBMLEnum { Name = name, Values = [] };
    }
}
