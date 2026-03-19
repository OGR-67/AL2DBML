namespace AL2DBML.Core.Models;

public class OutputSchema
{
    public List<DBMLEnum> Enums { get; set; } = [];
    public List<DBMLTable> Tables { get; set; } = [];
}
