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
    public string? InputPath { get; init; }

    [CommandOption("-o|--output <OUTPUT_PATH>")]
    [Description("The path to the output directory.")]
    public string? OutputPath { get; init; }

    [CommandOption("-n|--name <OUTPUT_NAME>")]
    [Description("The name of the output file (without extension).")]
    public string? OutputName { get; init; }
}

public class GenerateCommand : AsyncCommand<GenerateSettings>
{
    private readonly IAlParser _alParser;
    private readonly IDBMLWriter _dbmlWriter;
    private readonly IParsingTracker _tracker;
    private readonly IConfigService _configService;

    public GenerateCommand(IAlParser alParser, IDBMLWriter dbmlWriter, IParsingTracker tracker, IConfigService configService)
    {
        _alParser = alParser;
        _dbmlWriter = dbmlWriter;
        _tracker = tracker;
        _configService = configService;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, GenerateSettings settings, CancellationToken cancellationToken)
    {
        var sharedConfig = _configService.LoadSharedConfig();
        var localConfig = _configService.LoadLocalConfig();

        var inputPath = settings.InputPath ?? localConfig?.Input.Path ?? ".";
        var outputPath = settings.OutputPath ?? sharedConfig?.Output.Path ?? ".";
        var outputName = settings.OutputName ?? sharedConfig?.Output.Name ?? "schema";

        var inputType = FileSystemService.GetInputType(inputPath);
        if (inputType == Enums.InputType.NotSupported)
        {
            AnsiConsole.MarkupLine("[red]Error:[/] Unsupported input type. Please provide a valid directory, AL file, or vscode workspace file.");
            return -1;
        }

        var fullOutputPath = Path.Combine(outputPath, $"{outputName}.dbml");
        Directory.CreateDirectory(outputPath);

        var factory = new InputStrategyFactory(inputType, _alParser, _tracker);
        var outputSchema = factory.Strategy.Execute(inputPath);

        var dbmlContent = await _dbmlWriter.WriteDBMLAsync(outputSchema);
        await File.WriteAllTextAsync(fullOutputPath, dbmlContent, cancellationToken);

        AnsiConsole.MarkupLine($"[green]Done:[/] {_tracker.FileCount} file(s) parsed in {_tracker.Elapsed.TotalSeconds:F2}s → {Markup.Escape(fullOutputPath)}");

        return 0;
    }
}
