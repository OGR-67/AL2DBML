using AL2DBML.Application.Interfaces;
using AL2DBML.CLI.Services;
using AL2DBML.Core.Models;

namespace AL2DBML.CLI.Strategies;

public class FolderInputStrategy : IInputStrategy
{
    private readonly IAlParser _alParser;

    public FolderInputStrategy(IAlParser alParser)
    {
        _alParser = alParser;
    }

    public OutputSchema Execute(string inputPath)
    {
        var files = FileSystemService.ScanDirectory(inputPath);
        var singleFileStrategy = new SingleFileInputStrategy(_alParser);

        foreach (var file in files)
            singleFileStrategy.Execute(file);

        return _alParser.GetOutputSchema();
    }
}
