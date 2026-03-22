using AL2DBML.CLI.Constants;
using AL2DBML.CLI.Models;
using AL2DBML.CLI.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AL2DBML.CLI.Commands;

public class InitSettings : CommandSettings
{
}

public class InitCommand : AsyncCommand<InitSettings>
{
    private const string GitignoreEntry = ".al2dbml/config.local.json";

    private readonly IConfigService _configService;

    public InitCommand(IConfigService configService)
    {
        _configService = configService;
    }

    protected override Task<int> ExecuteAsync(CommandContext context, InitSettings settings, CancellationToken cancellationToken)
    {
        var existingShared = _configService.LoadSharedConfig();
        var existingLocal = _configService.LoadLocalConfig();

        if (_configService.ConfigExists())
            AnsiConsole.MarkupLine("[yellow]Existing config found — pre-filling with current values.[/]");

        var inputPath = AnsiConsole.Prompt(
            new TextPrompt<string>("Input path (you can point to a code-workspace file):")
                .DefaultValue(existingLocal?.Input.Path ?? "."));

        var outputPath = AnsiConsole.Prompt(
            new TextPrompt<string>("Output path:")
                .DefaultValue(existingShared?.Output.Path ?? "./docs/"));

        var outputName = AnsiConsole.Prompt(
            new TextPrompt<string>("Output file name (without extension):")
                .DefaultValue(existingShared?.Output.Name ?? "schema"));

        var hookExists = File.Exists(".git/hooks/pre-commit") &&
                         File.ReadAllText(".git/hooks/pre-commit").Contains(HookMarkers.Start, StringComparison.Ordinal);
        var createHook = AnsiConsole.Confirm("Create a pre-commit hook?", hookExists);

        _configService.SaveSharedConfig(new SharedConfig
        {
            Output = new OutputConfig { Path = outputPath, Name = outputName }
        });
        _configService.SaveLocalConfig(new LocalConfig
        {
            Input = new InputConfig { Path = inputPath }
        });

        EnsureGitignoreEntry(GitignoreEntry);

        if (createHook)
        {
            if (!Directory.Exists(".git/hooks"))
                AnsiConsole.MarkupLine("[yellow]Warning:[/] .git/hooks directory not found — skipping pre-commit hook creation.");
            else
                HookService.Write();
        }
        else if (hookExists)
        {
            AnsiConsole.MarkupLine("[yellow]Removing existing pre-commit hook...[/]");
            HookService.Remove();
        }

        AnsiConsole.MarkupLine("[green]Done:[/] AL2DBML initialized.");
        return Task.FromResult(0);
    }

    private static void EnsureGitignoreEntry(string entry)
    {
        const string gitignorePath = ".gitignore";
        var lines = File.Exists(gitignorePath)
            ? File.ReadAllLines(gitignorePath).ToList()
            : [];

        if (lines.Contains(entry))
            return;

        lines.Add(entry);
        File.WriteAllLines(gitignorePath, lines);
    }
}
