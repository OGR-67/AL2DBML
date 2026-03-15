using AL2DBML.Core.Models;

namespace AL2DBML.Application.Interfaces;

public interface IAlParser
{
    DBMLEnum ParseEnum(string alEnumFileContent);
}
