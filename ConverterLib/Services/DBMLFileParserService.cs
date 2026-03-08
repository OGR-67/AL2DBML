using System;
using System.Text.RegularExpressions;
using ConverterLib.Models;

namespace ConverterLib.Services;

// Class that takes the output schema and generates a DBML file from it
public class DBMLFileParserService
{
    private readonly OutputSchema _outputSchema;

    public DBMLFileParserService(OutputSchema outputSchema)
    {
        _outputSchema = outputSchema;
    }

    // Quote identifiers that contain spaces, dots, or any non-alphanumeric/underscore character
    private static string Q(string name) =>
        Regex.IsMatch(name, @"[^a-zA-Z0-9_]") ? $"\"{name}\"" : name;

    public string GenerateDBMLFile(string outputPath)
    {
        var outputString = string.Empty;

        outputString += ParseEnums();
        outputString += ParseTables();

        return outputString;
    }

    private string ParseTables()
    {
        var outputString = string.Empty;

        foreach (var table in _outputSchema.Tables)
        {
            outputString += $"table {Q(table.Name)} {{\n";
            foreach (var column in table.Columns)
            {
                outputString += $"  {Q(column.Name)} {Q(column.Type)}";

                var columnAttributes = new List<string>();

                if (column.IsPrimaryKey)
                {
                    columnAttributes.Add("pk");
                }
                if (column.References?[0] != null && column.References?[0] != "" && column.References.Length == 2)
                {
                    columnAttributes.Add($"ref: > {Q(column.References[0])}.{Q(column.References[1])}");
                }
                if (column.IsFlowfield && !string.IsNullOrEmpty(column.CalcFormula))
                {
                    var escapedFormula = column.CalcFormula.Replace("'", "\\'");
                    columnAttributes.Add($"note: 'FlowField: CalcFormula = {escapedFormula}'");
                }

                if (columnAttributes.Count > 0)
                {
                    outputString += " [" + string.Join(", ", columnAttributes) + "]";
                }

                outputString += "\n";
            }
            outputString += "}\n\n";
        }
        return outputString;
    }

    private string ParseEnums()
    {
        var outputString = string.Empty;

        foreach (var enumObj in _outputSchema.Enums)
        {
            outputString += $"enum {Q(enumObj.Name)} {{\n";
            foreach (var value in enumObj.Values)
            {
                outputString += $"  {Q(value)}\n";
            }
            outputString += "}\n\n";
        }

        return outputString;
    }
}
