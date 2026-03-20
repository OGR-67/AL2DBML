using AL2DBML.Core.Models;

namespace AL2DBML.Application.Interfaces;

public interface IDBMLWriter
{
    Task<string> WriteDBMLAsync(OutputSchema outputSchema);
}
