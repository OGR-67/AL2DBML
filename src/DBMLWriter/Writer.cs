using System.Text;
using System.Text.RegularExpressions;
using AL2DBML.Application.Interfaces;
using AL2DBML.Core.Models;

namespace DBMLWriter;

public class Writer : IDBMLWriter
{
    private readonly ISchemaPostProcessor _postProcessor;

    public Writer(ISchemaPostProcessor postProcessor)
    {
        _postProcessor = postProcessor;
    }

    public Task<string> WriteDBMLAsync(OutputSchema outputSchema)
    {
        var schema = _postProcessor.Process(outputSchema);

        var sb = new StringBuilder();
        sb.Append(WriteEnums(schema.Enums));
        sb.Append(WriteTables(schema.Tables));

        return Task.FromResult(sb.ToString());
    }

    private static string Quotes(string name) =>
        Regex.IsMatch(name, @"[^a-zA-Z0-9_]") ? $"\"{name}\"" : name;

    private static string WriteEnums(List<DBMLEnum> enums)
    {
        var sb = new StringBuilder();
        foreach (var enumObj in enums)
        {
            sb.AppendLine($"enum {Quotes(enumObj.Name)} {{");
            foreach (var value in enumObj.Values)
                sb.AppendLine($"  {Quotes(value)}");
            sb.AppendLine("}");
            sb.AppendLine();
        }
        return sb.ToString();
    }

    private static string WriteTables(List<DBMLTable> tables)
    {
        var sb = new StringBuilder();
        foreach (var table in tables)
        {
            sb.AppendLine($"table {Quotes(table.Name)} {{");
            foreach (var field in table.Fields)
            {
                sb.Append($"  {Quotes(field.Name)} {Quotes(field.Type)}");

                var attributes = new List<string>();

                if (field.IsPrimaryKey)
                    attributes.Add("pk");

                if (field.References is { Length: 2 } refs && !string.IsNullOrEmpty(refs[0]))
                    attributes.Add($"ref: > {Quotes(refs[0])}.{Quotes(refs[1])}");

                if (field.IsFlowfield && !string.IsNullOrEmpty(field.CalcFormula))
                    attributes.Add($"note: 'FlowField: CalcFormula = {field.CalcFormula.Replace("'", "\\'")}'");

                if (attributes.Count > 0)
                    sb.Append(" [" + string.Join(", ", attributes) + "]");

                sb.AppendLine();
            }
            sb.AppendLine("}");
            sb.AppendLine();
        }
        return sb.ToString();
    }
}
