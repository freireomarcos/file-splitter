namespace FileSplitter.Application.Contracts;

public interface IFilePathResolver
{
    string ResolveSolutionDirectory();
    string[] FindFiles(string directory, string extension);
    string GetRelativePath(string basePath, string fullPath);
}
