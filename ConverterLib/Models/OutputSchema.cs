using System;

namespace ConverterLib.Models;

public class OutputSchema
{
    public List<DBMLEnum> Enums { get; set; } = [];
    public List<DBMLTable> Tables { get; set; } = [];
}
