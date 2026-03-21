using AL2DBML.Application.Interfaces;
using AL2DBML.Core.Models;

namespace AL2DBML.CLI.Strategies;

class WorkspaceInputStrategy : IInputStrategy
{
    private readonly IAlParser _alParser;

    public WorkspaceInputStrategy(IAlParser alParser)
    {
        _alParser = alParser;
    }

    public OutputSchema Execute(string inputPath)
    {
        throw new NotImplementedException();
    }
}
