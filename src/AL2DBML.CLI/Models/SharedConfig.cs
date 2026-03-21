namespace AL2DBML.CLI.Models;

public class SharedConfig
{
    public OutputConfig Output { get; set; } = new();
}

public class OutputConfig
{
    public string Path { get; set; } = "./docs/";
    public string Name { get; set; } = "schema";
}
