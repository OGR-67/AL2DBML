using AL2DBML.Application.Helpers;
using AL2DBML.Application.Interfaces;
using AL2DBML.Core.Models;

namespace AL2DBML.DBMLWriter;

public class SchemaPostProcessor : ISchemaPostProcessor
{
    private const string Unknown = "UnknownField";

    public OutputSchema Process(OutputSchema schema)
    {
        var copy = OutputSchemaHelper.DeepCopy(schema);

        InferSinglePkWhenOnlyUnknownPlusOneColumn(copy);
        RemoveEmptyTables(copy);
        CreateStubsForMissingReferencedTables(copy);
        AddMissingReferencedFieldsToExistingTables(copy);

        var singlePkByTable = copy.Tables
            .Select(t => new { Table = t, Pks = t.Fields.Where(f => f.IsPrimaryKey).ToList() })
            .Where(x => x.Pks.Count == 1)
            .ToDictionary(x => x.Table.Name, x => x.Pks[0], StringComparer.OrdinalIgnoreCase);

        ResolveUnknownFieldReferences(copy, singlePkByTable);
        RemoveUnknownFieldColumns(copy, singlePkByTable);

        return copy;
    }

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
            table.Fields.RemoveAll(f => string.Equals(f.Name, Unknown, StringComparison.OrdinalIgnoreCase) && !f.IsPrimaryKey);
    }

    private static void CreateStubsForMissingReferencedTables(OutputSchema schema)
    {
        var existingTableNames = schema.Tables
            .Select(t => t.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var referencingFieldsByTable = schema.Tables
            .SelectMany(t => t.Fields)
            .Where(f => f.References is { Length: 2 } refs && !string.IsNullOrEmpty(refs[0]) && !existingTableNames.Contains(refs[0]))
            .GroupBy(f => f.References![0], StringComparer.OrdinalIgnoreCase);

        foreach (var group in referencingFieldsByTable)
        {
            var explicitFields = group
                .Where(f => !string.IsNullOrEmpty(f.References![1]) && !string.Equals(f.References![1], Unknown, StringComparison.OrdinalIgnoreCase))
                .DistinctBy(f => f.References![1], StringComparer.OrdinalIgnoreCase)
                .Select(f => new DBMLColumn { Name = f.References![1], Type = f.Type, IsPrimaryKey = true })
                .ToList();

            var stubFields = explicitFields.Count > 0
                ? explicitFields
                : [new DBMLColumn { Name = Unknown, Type = group.First().Type, IsPrimaryKey = true }];

            schema.Tables.Add(new DBMLTable { Name = group.Key, Fields = stubFields });
        }
    }

    private static void AddMissingReferencedFieldsToExistingTables(OutputSchema schema)
    {
        var tablesByName = schema.Tables.ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);

        var explicitRefs = schema.Tables
            .SelectMany(t => t.Fields)
            .Where(f => f.References is { Length: 2 } refs
                        && !string.IsNullOrEmpty(refs[0])
                        && !string.IsNullOrEmpty(refs[1])
                        && !string.Equals(refs[1], Unknown, StringComparison.OrdinalIgnoreCase)
                        && tablesByName.ContainsKey(refs[0]));

        foreach (var field in explicitRefs)
        {
            var targetTable = tablesByName[field.References![0]];
            var referencedFieldName = field.References![1];

            if (!targetTable.Fields.Any(f => string.Equals(f.Name, referencedFieldName, StringComparison.OrdinalIgnoreCase)))
                targetTable.Fields.Insert(0, new DBMLColumn { Name = referencedFieldName, Type = field.Type, IsPrimaryKey = true });
        }
    }

    private static void RemoveEmptyTables(OutputSchema schema)
    {
        schema.Tables.RemoveAll(t => t.Fields.Count == 0);
    }
}
