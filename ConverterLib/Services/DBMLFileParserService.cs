using System;
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
            var tableName = table.Name.Contains(' ') ? $"\"{table.Name}\"" : table.Name;
            outputString += $"table {tableName} {{\n";
            foreach (var column in table.Columns)
            {
                var columnName = (column.Name.Contains(' ') || column.Name.EndsWith(".")) ? $"\"{column.Name}\"" : column.Name;
                var columnType = (column.Type.Contains(' ') || column.Type.EndsWith(".")) ? $"\"{column.Type}\"" : column.Type;
                outputString += $"  {columnName} {columnType}";

                var columnAttributes = new List<string>();

                if (column.IsPrimaryKey)
                {
                    columnAttributes.Add("pk");
                }
                if (column.References?[0] != null && column.References?[0] != "" && column.References.Length == 2)
                {
                    var refTable = (column.References[0].Contains(' ') || column.References[0].EndsWith(".")) ? $"\"{column.References[0]}\"" : column.References[0];
                    var refColumn = (column.References[1].Contains(' ') || column.References[1].EndsWith(".")) ? $"\"{column.References[1]}\"" : column.References[1];
                    columnAttributes.Add($"ref: > {refTable}.{refColumn}");
                }
                if (column.IsFlowfield && !string.IsNullOrEmpty(column.CalcFormula))
                {
                    columnAttributes.Add($"note: 'FlowField: CalcFormula = {column.CalcFormula}'");
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
            var enumName = enumObj.Name.Contains(' ') ? $"\"{enumObj.Name}\"" : enumObj.Name;
            outputString += $"enum {enumName} {{\n";
            foreach (var value in enumObj.Values)
            {
                var cleanValue = value.Contains(' ') ? $"\"{value}\"" : value;
                outputString += $"  {cleanValue}\n";
            }
            outputString += "}\n\n";
        }

        return outputString;
    }
}
