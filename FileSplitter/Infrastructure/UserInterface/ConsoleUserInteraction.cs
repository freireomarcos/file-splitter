using FileSplitter.Application.Contracts;
using FileSplitter.Domain;

namespace FileSplitter.Infrastructure.UserInterface;

public sealed class ConsoleUserInteraction : IUserInteraction
{
    private readonly IFilePathResolver _filePathResolver;

    public ConsoleUserInteraction(IFilePathResolver filePathResolver)
    {
        _filePathResolver = filePathResolver;
    }

    public void DisplayWelcome()
    {
        Console.WriteLine("=== File Processor ===");
        Console.WriteLine("This program splits large files into smaller parts while preserving the header in each output file.\n");
    }

    public int GetLinesPerFile()
    {
        while (true)
        {
            Console.Write("Enter the number of lines per output file (or -1 for single file, ignoring blank lines): ");
            var input = Console.ReadLine();

            if (int.TryParse(input, out var linesPerFile))
            {
                if (linesPerFile == -1)
                {
                    Console.WriteLine("Mode selected: Single file output (all data lines, ignoring blank lines)");
                    return linesPerFile;
                }

                if (linesPerFile > 0)
                    return linesPerFile;
            }

            Console.WriteLine("Please enter a valid positive number or -1 for single file mode.");
        }
    }

    public FileType GetFileType()
    {
        while (true)
        {
            Console.WriteLine("\nSelect file type:");
            Console.WriteLine("1 - Excel file (.xlsx)");
            Console.WriteLine("2 - CSV file (.csv)");
            Console.Write("Enter your choice (1 or 2): ");

            var choice = Console.ReadLine();

            var fileType = choice switch
            {
                "1" => FileType.Excel,
                "2" => FileType.Csv,
                _ => FileType.Invalid
            };

            if (fileType != FileType.Invalid)
                return fileType;

            Console.WriteLine("Invalid option. Please enter 1 or 2.");
        }
    }

    public string GetFilePath(FileType fileType)
    {
        var extension = GetFileExtension(fileType);
        var solutionDirectory = _filePathResolver.ResolveSolutionDirectory();

        Console.WriteLine($"\nSearching for files in: {solutionDirectory}");

        var files = _filePathResolver.FindFiles(solutionDirectory, extension);

        if (files.Length == 0)
        {
            Console.WriteLine($"\nNo {extension} files found in the solution directory: {solutionDirectory}");
            Console.WriteLine("Please add a file to the solution directory and try again.");
            Environment.Exit(0);
        }

        DisplayAvailableFiles(solutionDirectory, files);
        return SelectFile(files);
    }

    public void DisplayProcessingStart(string fileName, int linesPerFile)
    {
        Console.WriteLine($"\nProcessing file: {fileName}");

        if (linesPerFile == -1)
            Console.WriteLine("Mode: Single file output (ignoring blank lines)");
        else
            Console.WriteLine($"Lines per file: {linesPerFile:N0}");

        Console.WriteLine("Please wait...\n");
    }

    public void DisplaySuccess()
    {
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    public void DisplayError(string message)
    {
        Console.WriteLine($"An error occurred: {message}");
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    public void WaitForExit()
    {
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    private static string GetFileExtension(FileType fileType) =>
        fileType == FileType.Excel ? "*.xlsx" : "*.csv";

    private void DisplayAvailableFiles(string baseDirectory, string[] files)
    {
        Console.WriteLine("\nAvailable files:");
        for (int i = 0; i < files.Length; i++)
        {
            var relativePath = _filePathResolver.GetRelativePath(baseDirectory, files[i]);
            Console.WriteLine($"{i + 1} - {relativePath}");
        }
    }

    private static string SelectFile(string[] files)
    {
        while (true)
        {
            Console.Write($"Select a file (1-{files.Length}): ");

            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                continue;

            if (int.TryParse(input, out var fileIndex) && fileIndex >= 1 && fileIndex <= files.Length)
                return files[fileIndex - 1];

            Console.WriteLine("Invalid selection. Please try again.");
        }
    }
}
