using AL2DBML.Core.Models;

namespace AL2DBML.CLI.Strategies;

public interface IInputStrategy
{
    OutputSchema Execute(string inputPath);
}
