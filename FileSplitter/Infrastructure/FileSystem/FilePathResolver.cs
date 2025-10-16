using FileSplitter.Application.Contracts;

namespace FileSplitter.Infrastructure.FileSystem;

public sealed class FilePathResolver : IFilePathResolver
{
    public string ResolveSolutionDirectory()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        
        if (!currentDirectory.Contains("bin"))
            return currentDirectory;

        var directory = new DirectoryInfo(currentDirectory);
        while (directory != null && !directory.GetFiles("*.sln").Any())
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? currentDirectory;
    }

    public string[] FindFiles(string directory, string extension)
    {
        return Directory.GetFiles(directory, extension, SearchOption.AllDirectories)
            .Where(f => !f.Contains("\\bin\\") && !f.Contains("\\obj\\"))
            .ToArray();
    }

    public string GetRelativePath(string basePath, string fullPath)
    {
        return Path.GetRelativePath(basePath, fullPath);
    }
}
