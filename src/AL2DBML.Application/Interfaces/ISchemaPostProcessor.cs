using AL2DBML.Core.Models;

namespace AL2DBML.Application.Interfaces;

public interface ISchemaPostProcessor
{
    OutputSchema Process(OutputSchema schema);
}
