using AL2DBML.Application.Interfaces;
using AL2DBML.Core.Models;

namespace DBMLWriter;

public class SchemaPostProcessor : ISchemaPostProcessor
{
    private const string Unknown = "UnknownField";

    public OutputSchema Process(OutputSchema schema)
    {
        var copy = DeepCopy(schema);

        InferSinglePkWhenOnlyUnknownPlusOneColumn(copy);

        var singlePkByTable = copy.Tables
            .Select(t => new { Table = t, Pks = t.Fields.Where(f => f.IsPrimaryKey).ToList() })
            .Where(x => x.Pks.Count == 1)
            .ToDictionary(x => x.Table.Name, x => x.Pks[0], StringComparer.OrdinalIgnoreCase);

        ResolveUnknownFieldReferences(copy, singlePkByTable);
        RemoveUnknownFieldColumns(copy, singlePkByTable);

        return copy;
    }

    private static OutputSchema DeepCopy(OutputSchema schema) =>
        new()
        {
            Enums = schema.Enums
                .Select(e => new DBMLEnum { Name = e.Name, Values = [..e.Values] })
                .ToList(),
            Tables = schema.Tables
                .Select(t => new DBMLTable
                {
                    Name = t.Name,
                    Fields = t.Fields
                        .Select(f => new DBMLColumn
                        {
                            Name = f.Name,
                            Type = f.Type,
                            IsPrimaryKey = f.IsPrimaryKey,
                            References = f.References?.ToArray(),
                            IsFlowfield = f.IsFlowfield,
                            CalcFormula = f.CalcFormula
                        })
                        .ToList()
                })
                .ToList()
        };

    private static void InferSinglePkWhenOnlyUnknownPlusOneColumn(OutputSchema schema)
    {
        foreach (var table in schema.Tables)
        {
            if (table.Fields.Count == 0 || table.Fields.Any(f => f.IsPrimaryKey)) continue;

            var realFields = table.Fields
                .Where(f => !string.Equals(f.Name, Unknown, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var hasUnknown = table.Fields.Any(f => string.Equals(f.Name, Unknown, StringComparison.OrdinalIgnoreCase));

            if (realFields.Count == 1 && hasUnknown)
                realFields[0].IsPrimaryKey = true;
        }
    }

    private static void ResolveUnknownFieldReferences(OutputSchema schema, Dictionary<string, DBMLColumn> singlePkByTable)
    {
        foreach (var table in schema.Tables)
        {
            foreach (var field in table.Fields)
            {
                if (field.References is not { Length: 2 } refs) continue;
                if (!string.Equals(refs[1], Unknown, StringComparison.OrdinalIgnoreCase)) continue;
                if (!singlePkByTable.TryGetValue(refs[0], out var pkField)) continue;

                field.References[1] = pkField.Name;
                if (!string.IsNullOrWhiteSpace(pkField.Type))
                    field.Type = pkField.Type;
            }
        }
    }

    private static void RemoveUnknownFieldColumns(OutputSchema schema, Dictionary<string, DBMLColumn> singlePkByTable)
    {
        foreach (var table in schema.Tables.Where(t => singlePkByTable.ContainsKey(t.Name)))
            table.Fields.RemoveAll(f => string.Equals(f.Name, Unknown, StringComparison.OrdinalIgnoreCase));
    }
}
