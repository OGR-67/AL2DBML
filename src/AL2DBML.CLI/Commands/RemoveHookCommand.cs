using AL2DBML.CLI.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AL2DBML.CLI.Commands;

public class RemoveHookSettings : CommandSettings
{
}

public class RemoveHookCommand : Command<RemoveHookSettings>
{
    protected override int Execute(CommandContext context, RemoveHookSettings settings, CancellationToken cancellationToken)
    {
        var removed = HookService.Remove();
        if (!removed)
            AnsiConsole.MarkupLine("[yellow]Warning:[/] No AL2DBML section found in pre-commit hook.");
        else
            AnsiConsole.MarkupLine("[green]Done:[/] AL2DBML section removed from pre-commit hook.");

        return 0;
    }
}
