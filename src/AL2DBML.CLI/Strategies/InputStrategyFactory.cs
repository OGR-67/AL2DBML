using AL2DBML.Application.Interfaces;
using AL2DBML.CLI.Enums;
using AL2DBML.CLI.Services;

namespace AL2DBML.CLI.Strategies;

public class InputStrategyFactory
{
    public IInputStrategy Strategy { get; }
    public InputStrategyFactory(InputType inputType, IAlParser alParser, IParsingTracker tracker)
    {
        switch (inputType)
        {
            case InputType.Directory:
                Strategy = new FolderInputStrategy(alParser, tracker);
                break;
            case InputType.ALFile:
                Strategy = new SingleFileInputStrategy(alParser, tracker);
                break;
            case InputType.WorkspaceFile:
                Strategy = new WorkspaceInputStrategy(alParser, tracker);
                break;
            default:
                throw new NotSupportedException($"Input type {inputType} is not supported.");
        }
    }
}
