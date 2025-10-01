using AL2DBML.Commands;
using Spectre.Console.Cli;

internal class Program
{
    private static void Main(string[] args)
    {
        var app = new CommandApp<ConvertCommand>();
        app.Run(args);
    }
}