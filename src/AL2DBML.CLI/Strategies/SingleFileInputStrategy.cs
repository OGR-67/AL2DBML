using AL2DBML.Application.Interfaces;
using AL2DBML.Core.Enums;
using AL2DBML.Core.Models;

namespace AL2DBML.CLI.Strategies;

public class SingleFileInputStrategy : IInputStrategy
{
    private readonly IAlParser _alParser;
    public SingleFileInputStrategy(IAlParser alParser)
    {
        _alParser = alParser;
    }
    public OutputSchema Execute(string inputPath)
    {
        var content = File.ReadAllText(inputPath);
        switch (_alParser.DetectFileType(content))
        {
            case AlFileType.Enum:
                _alParser.ParseEnum(content);
                break;
            case AlFileType.EnumExtension:
                _alParser.ParseEnumExtension(content);
                break;
            case AlFileType.Table:
                _alParser.ParseTable(content);
                break;
            case AlFileType.TableExtension:
                _alParser.ParseTableExtension(content);
                break;
            // Unknown: skip silently (unsupported files in a folder)
        }
        return _alParser.GetOutputSchema();
    }
}
