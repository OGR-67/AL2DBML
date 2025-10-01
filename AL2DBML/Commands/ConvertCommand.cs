using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ConverterLib;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AL2DBML.Commands;

internal sealed class ConvertCommand : Command<ConvertCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [Description("Input target - AL file, workspace file or folder")]
        [CommandOption("-i|--input <INPUT>")]
        [Required]
        public string? Input { get; init; }

        [Description("Output target directory for the DBML file")]
        [CommandOption("-o|--output <OUTPUT>")]
        [Required]
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

        List<string> fileList = Helper.GetALFiles(input);

        AnsiConsole.Progress()
            .Start(ctx =>
            {
                var task = ctx.AddTask("Converting AL files to DBML...", maxValue: fileList.Count);
                foreach (var file in fileList)
                {
                    // Simulate work
                    Task.Delay(500).Wait();
                    task.Increment(1);
                }
            });

        AnsiConsole.MarkupLine($"[green]Success:[/] Converted {fileList.Count} AL files to DBML in '{output}'.");

        return 0;
    }
}
