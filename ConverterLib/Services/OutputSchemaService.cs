using System;
using ConverterLib.Models;

namespace ConverterLib.Services;

public class OutputSchemaService
{
    public OutputSchema schema { get; set; } = new OutputSchema();

    public void AddTable(DBMLTable table)
    {
        // Check if table already exists. Enum extensions may have created the table already.
        // We have to add missing columns in that case.
        var existingTable = schema.Tables.FirstOrDefault(t => t.Name == table.Name);
        if (existingTable != null)
        {
            foreach (var column in table.Columns)
            {
                if (!existingTable.Columns.Any(c => c.Name == column.Name))
                {
                    existingTable.Columns.Add(column);
                }
            }
            return;
        }
        schema.Tables.Add(table);
    }

    public void AddColumnToTable(string tableName, DBMLColumn column)
    {
        var table = schema.Tables.FirstOrDefault(t => t.Name == tableName);
        if (table != null)
        {
            table.Columns.Add(column);
            return;
        }
        var newTable = new DBMLTable
        {
            Name = tableName,
            Columns = new List<DBMLColumn> { column }
        };
        schema.Tables.Add(newTable);
    }

    public void AddEnum(DBMLEnum dbmlEnum)
    {
        // Check if enum already exists
        // Enum extensions may have created the enum already.
        var existingEnum = schema.Enums.FirstOrDefault(e => e.Name == dbmlEnum.Name);
        if (existingEnum != null)
        {
            // Merge values
            foreach (var value in dbmlEnum.Values)
            {
                if (!existingEnum.Values.Contains(value))
                {
                    existingEnum.Values.Add(value);
                }
            }
            return;
        }
        schema.Enums.Add(dbmlEnum);
    }

    public void AddEnumExtension(string enumName, List<string> values)
    {
        var existingEnum = schema.Enums.FirstOrDefault(e => e.Name == enumName);

        if (existingEnum != null)
        {
            existingEnum.Values.AddRange(values);
            return;
        }
        var newEnum = new DBMLEnum
        {
            Name = enumName,
            Values = values
        };
        schema.Enums.Add(newEnum);
    }
}
