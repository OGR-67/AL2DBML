using System.Text.RegularExpressions;

internal class AlSyntaxHelper
{
    public static string CleanName(string name)
    {
        if (name.Contains('/') && !name.StartsWith('"'))
            return $"\"{name}\"";
        return name.Replace("\"", "");
    }

    public static string ExtractMatch(string content, string pattern, int groupIndex = 1, string? context = null)
    {
        var match = Regex.Match(content, pattern, RegexOptions.Multiline);
        return match.Success
            ? CleanName(match.Groups[groupIndex].Value)
            : throw new FormatException($"Pattern not found{(context != null ? $" in {context}" : "")}");
    }

    public static List<string> ExtractAllMatches(string content, string pattern, int groupIndex = 1) =>
        Regex.Matches(content, pattern, RegexOptions.Multiline)
            .Select(m => CleanName(m.Groups[groupIndex].Value))
            .ToList();
}
