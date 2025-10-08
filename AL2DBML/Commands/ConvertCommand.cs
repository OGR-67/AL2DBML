using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ConverterLib;
using ConverterLib.Models;
using ConverterLib.Services;
using Newtonsoft.Json;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Json;

namespace AL2DBML.Commands;

internal sealed class ConvertCommand : Command<ConvertCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [Description("Input target - AL file, workspace file or folder")]
        [CommandOption("-i|--input <INPUT>")]
        public string? Input { get; init; }

        [Description("Output target directory for the DBML file")]
        [CommandOption("-o|--output <OUTPUT>")]
        public string? Output { get; init; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var input = settings.Input ?? ".";
        var output = settings.Output ?? ".";

        if (input.StartsWith('.'))
        {
            input = Path.Join(Directory.GetCurrentDirectory(), input);
        }

        if (output.StartsWith('.'))
        {
            output = Path.Join(Directory.GetCurrentDirectory(), output);
        }

        if (!Directory.Exists(output))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] Output directory '{output}' does not exist.");
            return -1;
        }

        List<string> fileList = Helper.GetALFilesToConvert(input);
        var outputSchema = new OutputSchema();

        AnsiConsole.Progress()
            .Start(ctx =>
            {
                var task = ctx.AddTask("Converting AL files to DBML...", maxValue: fileList.Count);
                foreach (var file in fileList)
                {
                    var parser = new AlFileParserService(file);
                    parser.ParseFile(ref outputSchema);
                    task.Increment(1);
                }
            });


        SchemaPostProcessing.CleanupUnknownFieldReferences(ref outputSchema);
        // serialise outputSchema to JSON using Newtonsoft.Json
        // var json = JsonConvert.SerializeObject(outputSchema, Formatting.Indented);

        // pretty print unsing spectre console
        // AnsiConsole.Write(new JsonText(json));

        // Write a JSON file of the output schema - Debug purpose
        // var outputFile = Path.Join(output, "schema.json");
        // File.WriteAllText(outputFile, json);

        var dbmlParser = new DBMLFileParserService(outputSchema);
        var dbmlContent = dbmlParser.GenerateDBMLFile(output);
        var dbmlOutputFile = Path.Join(output, "schema.dbml");

        File.WriteAllText(dbmlOutputFile, dbmlContent);

        AnsiConsole.MarkupLine($"[green]Success:[/] Converted {fileList.Count} AL files to DBML in '{dbmlOutputFile}'.");
        return 0;
    }
}
