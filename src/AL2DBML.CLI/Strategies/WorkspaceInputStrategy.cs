using System.Text.Json;
using AL2DBML.Application.Interfaces;
using AL2DBML.CLI.Services;
using AL2DBML.Core.Models;
using Spectre.Console;

namespace AL2DBML.CLI.Strategies;

class WorkspaceInputStrategy : IInputStrategy
{
    private readonly IAlParser _alParser;
    private readonly IParsingTracker _tracker;

    public WorkspaceInputStrategy(IAlParser alParser, IParsingTracker tracker)
    {
        _alParser = alParser;
        _tracker = tracker;
    }

    public OutputSchema Execute(string inputPath)
    {
        // Read the vscode workspace file to get the list of projects
        var workspaceContent = File.ReadAllText(inputPath);
        using var workspaceJson = JsonDocument.Parse(workspaceContent);
        if (!workspaceJson.RootElement.TryGetProperty("folders", out var folders))
        {
            throw new InvalidDataException("Invalid workspace file: 'folders' property not found.");
        }
        if (folders.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidDataException("Invalid workspace file: 'folders' property must be an array.");
        }
        foreach (var folder in folders.EnumerateArray())
        {
            if (!folder.TryGetProperty("path", out var path))
            {
                AnsiConsole.MarkupLine($"[orange]Warning:[/] A folder entry has no 'path' property. Skipping.");
                continue; // Skip if no path property
            }
            var projectPath = Path.Combine(Path.GetDirectoryName(inputPath) ?? string.Empty, path.GetString() ?? string.Empty);
            if (Directory.Exists(projectPath))
            {
                var folderStrategy = new FolderInputStrategy(_alParser, _tracker);
                folderStrategy.Execute(projectPath);
            }
            else
            {
                AnsiConsole.MarkupLine($"[orange]Warning:[/] Project path '{Markup.Escape(projectPath)}' does not exist. Skipping this entry.");
            }
        }
        return _alParser.GetOutputSchema();
    }
}
