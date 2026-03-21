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
    [Description("The path to the output file or directory.")]
    public string OutputPath { get; init; } = ".";

    [CommandOption("-n|--name <OUTPUT_NAME>")]
    [Description("The name of the output file (without extension). 'schema' by default.")]
    public string OutputName { get; init; } = "schema";
}

public class GenerateCommand : AsyncCommand<GenerateSettings>
{
    private readonly IAlParser _alParser;
    private readonly IDBMLWriter _dbmlWriter;

    public GenerateCommand(IAlParser alParser, IDBMLWriter dbmlWriter)
    {
        _alParser = alParser;
        _dbmlWriter = dbmlWriter;
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

        var factory = new InputStrategyFactory(inputType, _alParser);
        var outputSchema = factory.Strategy.Execute(settings.InputPath);

        var dbmlContent = await _dbmlWriter.WriteDBMLAsync(outputSchema);
        File.WriteAllText(outputPath, dbmlContent);

        return 0;
    }
}
