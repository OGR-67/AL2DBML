using AL2DBML.Application.Interfaces;
using AL2DBML.CLI.Services;
using AL2DBML.Core.Enums;
using AL2DBML.Core.Models;

namespace AL2DBML.CLI.Strategies;

public class SingleFileInputStrategy : IInputStrategy
{
    private readonly IAlParser _alParser;
    private readonly IParsingTracker _tracker;

    public SingleFileInputStrategy(IAlParser alParser, IParsingTracker tracker)
    {
        _alParser = alParser;
        _tracker = tracker;
    }

    public OutputSchema Execute(string inputPath)
    {
        var content = File.ReadAllText(inputPath);
        var fileType = _alParser.DetectFileType(content);
        switch (fileType)
        {
            case AlFileType.Enum:
                _alParser.ParseEnum(content);
                _tracker.RecordFile();
                break;
            case AlFileType.EnumExtension:
                _alParser.ParseEnumExtension(content);
                _tracker.RecordFile();
                break;
            case AlFileType.Table:
                _alParser.ParseTable(content);
                _tracker.RecordFile();
                break;
            case AlFileType.TableExtension:
                _alParser.ParseTableExtension(content);
                _tracker.RecordFile();
                break;
            // Unknown: skip silently (unsupported files in a folder)
        }
        return _alParser.GetOutputSchema();
    }
}
