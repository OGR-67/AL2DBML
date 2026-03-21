using System.Diagnostics;

namespace AL2DBML.CLI.Services;

public class ParsingTracker : IParsingTracker
{
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private int _fileCount;

    public void RecordFile() => _fileCount++;
    public int FileCount => _fileCount;
    public TimeSpan Elapsed => _stopwatch.Elapsed;
}
