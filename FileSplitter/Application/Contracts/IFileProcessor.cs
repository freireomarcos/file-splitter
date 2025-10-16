namespace FileSplitter.Application.Contracts;

public interface IFileProcessor
{
    Task ProcessAsync(string filePath, int linesPerFile);
}