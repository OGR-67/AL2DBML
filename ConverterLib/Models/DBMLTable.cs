using System;

namespace ConverterLib.Models;

public class DBMLTable
{
    public string Name { get; set; } = string.Empty;
    public List<DBMLColumn> Columns { get; set; } = [];
}
