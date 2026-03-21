using AL2DBML.CLI.Enums;

namespace AL2DBML.CLI.Services;

public static class FileSystemService
{
    public static InputType GetInputType(string path)
    {
        if (Directory.Exists(path))
            return InputType.Directory;

        if (File.Exists(path))
        {
            var extension = Path.GetExtension(path);
            if (extension.Equals(".al", StringComparison.OrdinalIgnoreCase))
                return InputType.ALFile;
            if (extension.Equals(".code-workspace", StringComparison.OrdinalIgnoreCase))
                return InputType.WorkspaceFile;
        }

        return InputType.NotSupported;
    }

    public static List<string> ScanDirectory(string directoryPath)
    {
        var paths = new List<string>();
        var files = Directory.GetFiles(directoryPath, "*.al", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            paths.Add(file);
        }

        return paths;
    }
}
