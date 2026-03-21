using System.ComponentModel;
using AL2DBML.Application.Interfaces;
using AL2DBML.CLI.Services;
using AL2DBML.CLI.Strategies;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AL2DBML.CLI.Commands;

public class GenerateSettings : CommandSettings
{
    [CommandOption("-i|--input <INPUT_PATH>")]
    [Description("The path to the AL project folder, AL file, or vscode workspace file of AL projects to parse.")]
    public string InputPath { get; init; } = ".";

    [CommandOption("-o|--output <OUTPUT_PATH>")]
    [Description("The path to the output directory.")]
    public string OutputPath { get; init; } = ".";

    [CommandOption("-n|--name <OUTPUT_NAME>")]
    [Description("The name of the output file (without extension). 'schema' by default.")]
    public string OutputName { get; init; } = "schema";
}

public class GenerateCommand : AsyncCommand<GenerateSettings>
{
    private readonly IAlParser _alParser;
    private readonly IDBMLWriter _dbmlWriter;
    private readonly IParsingTracker _tracker;

    public GenerateCommand(IAlParser alParser, IDBMLWriter dbmlWriter, IParsingTracker tracker)
    {
        _alParser = alParser;
        _dbmlWriter = dbmlWriter;
        _tracker = tracker;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, GenerateSettings settings, CancellationToken cancellationToken)
    {
        var inputType = FileSystemService.GetInputType(settings.InputPath);
        if (inputType == Enums.InputType.NotSupported)
        {
            AnsiConsole.MarkupLine("[red]Error:[/] Unsupported input type. Please provide a valid directory, AL file, or vscode workspace file.");
            return -1;
        }

        var outputPath = Path.Combine(settings.OutputPath, $"{settings.OutputName}.dbml");
        Directory.CreateDirectory(settings.OutputPath);

        var factory = new InputStrategyFactory(inputType, _alParser, _tracker);
        var outputSchema = factory.Strategy.Execute(settings.InputPath);

        var dbmlContent = await _dbmlWriter.WriteDBMLAsync(outputSchema);
        await File.WriteAllTextAsync(outputPath, dbmlContent, cancellationToken);

        AnsiConsole.MarkupLine($"[green]Done:[/] {_tracker.FileCount} file(s) parsed in {_tracker.Elapsed.TotalSeconds:F2}s → {Markup.Escape(outputPath)}");

        return 0;
    }
}
