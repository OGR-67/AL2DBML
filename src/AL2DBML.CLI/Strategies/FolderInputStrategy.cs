using AL2DBML.Application.Interfaces;
using AL2DBML.CLI.Services;
using AL2DBML.Core.Models;

namespace AL2DBML.CLI.Strategies;

public class FolderInputStrategy : IInputStrategy
{
    private readonly IAlParser _alParser;
    private readonly IParsingTracker _tracker;

    public FolderInputStrategy(IAlParser alParser, IParsingTracker tracker)
    {
        _alParser = alParser;
        _tracker = tracker;
    }

    public OutputSchema Execute(string inputPath)
    {
        var files = FileSystemService.ScanDirectory(inputPath);
        var singleFileStrategy = new SingleFileInputStrategy(_alParser, _tracker);

        foreach (var file in files)
            singleFileStrategy.Execute(file);

        return _alParser.GetOutputSchema();
    }
}
