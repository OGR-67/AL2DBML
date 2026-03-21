namespace AL2DBML.CLI.Models;

public class LocalConfig
{
    public InputConfig Input { get; set; } = new();
}

public class InputConfig
{
    public string Path { get; set; } = ".";
}
