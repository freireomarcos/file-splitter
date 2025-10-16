namespace FileSplitter.Domain;

public sealed class FileProcessingOptions
{
    public string FilePath { get; init; } = string.Empty;
    public int LinesPerFile { get; init; }
    public FileType FileType { get; init; }

    public bool IsSingleFileMode => LinesPerFile == -1;
}
