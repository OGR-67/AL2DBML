using AL2DBML.Core.Enums;
using AL2DBML.Core.Models;

namespace AL2DBML.Application.Interfaces;

public interface IAlParser
{
    AlFileType DetectFileType(string alFileContent);
    DBMLEnum ParseEnum(string alEnumFileContent);
    DBMLEnum ParseEnumExtension(string alEnumExtensionFileContent);
}
