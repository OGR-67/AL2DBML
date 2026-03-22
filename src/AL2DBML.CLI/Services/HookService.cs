using AL2DBML.CLI.Constants;

namespace AL2DBML.CLI.Services;

internal static class HookService
{
    private const string HookPath = ".git/hooks/pre-commit";
    private const string HookCommand = "al2dbml generate";

    public static void Write()
    {
        if (!Directory.Exists(".git/hooks"))
            return;

        var hookSection = $"{HookMarkers.Start}\nif command -v al2dbml > /dev/null 2>&1; then\n    {HookCommand} || printf \"\\033[33mWarning: al2dbml generate failed, skipping DBML update.\\033[0m\\n\"\nelse\n    printf \"\\033[33mWarning: al2dbml not found, skipping DBML update.\\033[0m\\n\"\nfi\n{HookMarkers.End}";

        string content;
        if (File.Exists(HookPath))
        {
            content = File.ReadAllText(HookPath);
            var startIdx = content.IndexOf(HookMarkers.Start, StringComparison.Ordinal);
            var endIdx = content.IndexOf(HookMarkers.End, StringComparison.Ordinal);

            if (startIdx >= 0 && endIdx >= 0)
                content = content[..startIdx] + hookSection + content[(endIdx + HookMarkers.End.Length)..];
            else
                content = content.TrimEnd() + $"\n\n{hookSection}\n";
        }
        else
        {
            content = $"#!/bin/sh\n\n{hookSection}\n";
        }

        File.WriteAllText(HookPath, content);

        if (!OperatingSystem.IsWindows())
            File.SetUnixFileMode(HookPath,
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                UnixFileMode.GroupRead | UnixFileMode.GroupExecute |
                UnixFileMode.OtherRead | UnixFileMode.OtherExecute);
    }

    public static bool Remove()
    {
        if (!File.Exists(HookPath)) return false;

        var content = File.ReadAllText(HookPath);
        var startIdx = content.IndexOf(HookMarkers.Start, StringComparison.Ordinal);
        var endIdx = content.IndexOf(HookMarkers.End, StringComparison.Ordinal);
        if (startIdx < 0 || endIdx < 0) return false;

        var before = content[..startIdx].TrimEnd();
        var after = content[(endIdx + HookMarkers.End.Length)..].TrimStart('\r', '\n');
        var newContent = before.Length > 0 && after.Length > 0
            ? before + "\n\n" + after
            : (before + after).Trim();

        if (string.IsNullOrWhiteSpace(newContent.Replace("#!/bin/sh", "")))
            File.Delete(HookPath);
        else
            File.WriteAllText(HookPath, newContent + "\n");

        return true;
    }
}
