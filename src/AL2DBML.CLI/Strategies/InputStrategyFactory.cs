using AL2DBML.Application.Interfaces;
using AL2DBML.CLI.Enums;

namespace AL2DBML.CLI.Strategies;

public class InputStrategyFactory
{
    public IInputStrategy Strategy { get; init; }
    public InputStrategyFactory(InputType inputType, IAlParser alParser)
    {
        switch (inputType)
        {
            case InputType.Directory:
                Strategy = new FolderInputStrategy(alParser);
                break;
            case InputType.ALFile:
                Strategy = new SingleFileInputStrategy(alParser);
                break;
            case InputType.WorkspaceFile:
                Strategy = new WorkspaceInputStrategy(alParser);
                break;
            default:
                throw new NotSupportedException($"Input type {inputType} is not supported.");
        }
    }
}
