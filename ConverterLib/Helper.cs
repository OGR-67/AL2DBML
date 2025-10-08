using System.Text.RegularExpressions;
using ConverterLib.Models;
using Newtonsoft.Json;

namespace ConverterLib;

public class Helper
{
    public static List<string> GetALFilesToConvert(string inputPath)
    {
        List<string> files = new List<string>();

        if (inputPath.EndsWith(".al", StringComparison.OrdinalIgnoreCase))
        {
            if (!File.Exists(inputPath))
            {
                throw new FileNotFoundException($"Input file '{inputPath}' does not exist.");
            }
            if (!IsSchemaDefinitionFile(inputPath))
            {
                throw new ArgumentException($"Input file '{inputPath}' is not a valid AL schema definition file.");
            }
            return [inputPath];
        }
        if (inputPath.EndsWith(".code-workspace", StringComparison.OrdinalIgnoreCase))
        {
            files = GetFilesFromCodeWorkspace(inputPath);
        }
        if (Directory.Exists(inputPath))
        {
            files = GetFilesFromDirectory(inputPath);
        }
        return files;
    }

    private static List<string> GetFilesFromCodeWorkspace(string inputPath)
    {
        if (!File.Exists(inputPath))
        {
            throw new FileNotFoundException($"Input file '{inputPath}' does not exist.");
        }
        var directories = new List<string>();

        var jsonContent = File.ReadAllText(inputPath);
        var workspace = JsonConvert.DeserializeObject<Workspace>(jsonContent);
        if (workspace == null)
        {
            throw new ArgumentException($"Input file '{inputPath}' is not a valid VSCode workspace file.");
        }

        directories.AddRange(workspace.Folders.Select(f => Path.Join(Directory.GetParent(inputPath)?.FullName ?? string.Empty, f.Path)));

        // log each directory found in the workspace
        directories.ForEach(d => Console.WriteLine($"Found directory in workspace: {d}"));

        var files = new List<string>();

        foreach (var dir in directories)
        {
            files.AddRange(GetFilesFromDirectory(dir));
        }
        return files;
    }

    private static List<string> GetFilesFromDirectory(string inputPath)
    {
        var files = new List<string>();
        if (!Directory.Exists(inputPath))
        {
            throw new DirectoryNotFoundException($"Input directory '{inputPath}' does not exist.");
        }

        var dirFiles = Directory.GetFiles(inputPath, "*.al", SearchOption.AllDirectories);
        files.AddRange(dirFiles.Where(IsSchemaDefinitionFile));

        return files;
    }

    private static bool IsSchemaDefinitionFile(string filePath)
    {
        // var regex = new Regex(@"^\s*(table)\s+\d+\s+(""[^""]+""|\S+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        var regex = new Regex(@"^\s*(table|tableextension|enum|enumextension)\s+\d+\s+(""[^""]+""|\S+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        var content = File.ReadAllText(filePath);

        return regex.IsMatch(content);
    }
}
