namespace AL2DBML.CLI.Services;

public interface IParsingTracker
{
    void RecordFile();
    int FileCount { get; }
    TimeSpan Elapsed { get; }
}
