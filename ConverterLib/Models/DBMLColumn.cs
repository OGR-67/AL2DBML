using System;

namespace ConverterLib.Models;

public class DBMLColumn
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsPrimaryKey { get; set; } = false;
    public bool IsNullable { get; set; } = true;
    public string DefaultValue { get; set; } = string.Empty;
    public string[]? References { get; set; } = new string[2];
    public bool IsFlowfield { get; set; } = false;
    public string CalcFormula { get; set; } = string.Empty;
}
