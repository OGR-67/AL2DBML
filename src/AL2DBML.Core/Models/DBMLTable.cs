namespace AL2DBML.Core.Models;

public class DBMLTable
{
    public string Name { get; set; } = string.Empty;
    public List<DBMLColumn> Fields { get; set; } = [];
}
