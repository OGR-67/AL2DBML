using AL2DBML.Core.Models;

namespace AL2DBML.Application.Helpers;

public static class OutputSchemaHelper
{
    public static OutputSchema DeepCopy(OutputSchema schema) =>
        new()
        {
            Enums = schema.Enums
                .Select(e => new DBMLEnum { Name = e.Name, Values = [.. e.Values] })
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

}
