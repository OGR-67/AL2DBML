using ConverterLib.Models;

namespace ConverterLib.Services
{
    public static class SchemaPostProcessing
    {
        private const string Unknown = "UnknownField";

        public static void CleanupUnknownFieldReferences(ref OutputSchema schema)
        {
            if (schema?.Tables == null) return;

            // Step 0: Infer PK if the table has no PK and has exactly one "real" field + UnknownField
            InferSinglePkWhenOnlyUnknownPlusOneColumn(schema);

            // 1) Map tables that have exactly one PK (including those inferred above)
            var singlePkByTable = schema.Tables
                .Select(t => new
                {
                    Table = t,
                    Pks = t.Columns?.Where(c => c.IsPrimaryKey).ToList() ?? new List<DBMLColumn>()
                })
                .Where(x => x.Pks.Count == 1)
                .ToDictionary(
                    x => x.Table.Name,
                    x => x.Pks[0],
                    StringComparer.OrdinalIgnoreCase);

            // 2) Replace in all FKs [RefTable, UnknownField] -> [RefTable, PK]
            foreach (var table in schema.Tables)
            {
                if (table?.Columns == null) continue;

                foreach (var col in table.Columns)
                {
                    if (col?.References == null || col.References.Count() != 2) continue;

                    var refTableName = col.References[0];
                    var refFieldName = col.References[1];

                    if (!string.Equals(refFieldName, Unknown, StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (!singlePkByTable.TryGetValue(refTableName, out var pkCol))
                        continue;

                    // Replace UnknownField with the actual PK
                    col.References[1] = pkCol.Name;

                    // Align the FK type with that of the PK
                    if (!string.IsNullOrWhiteSpace(pkCol.Type))
                        col.Type = pkCol.Type;
                }
            }

            // 3) Remove "UnknownField" from tables with a single PK
            foreach (var table in schema.Tables)
            {
                if (table?.Columns == null) continue;

                if (singlePkByTable.ContainsKey(table.Name))
                {
                    table.Columns.RemoveAll(c =>
                        c != null && string.Equals(c.Name, Unknown, StringComparison.OrdinalIgnoreCase));
                }
            }
        }

        private static void InferSinglePkWhenOnlyUnknownPlusOneColumn(OutputSchema schema)
        {
            foreach (var table in schema.Tables)
            {
                if (table?.Columns == null || table.Columns.Count == 0) continue;

                if (table.Columns.Any(c => c.IsPrimaryKey)) continue;

                var unknownCols = table.Columns
                    .Where(c => c != null && string.Equals(c.Name, Unknown, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var realCols = table.Columns
                    .Where(c => c != null && !string.Equals(c.Name, Unknown, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (realCols.Count == 1 && unknownCols.Count >= 1)
                {
                    var inferredPk = realCols[0];
                    inferredPk.IsPrimaryKey = true;
                }
            }
        }
    }
}
