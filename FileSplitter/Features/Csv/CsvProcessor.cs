using FileSplitter.Application.Contracts;

namespace FileSplitter.Features.Csv;

public sealed class CsvProcessor : IFileProcessor
{
    public async Task ProcessAsync(string filePath, int linesPerFile)
    {
        ValidateFile(filePath);

        var fileDirectory = Path.GetDirectoryName(filePath) ?? "";
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var outputDirectory = Path.Combine(fileDirectory, $"output_{fileName}");

        DisplayProcessingInfo(Path.GetFileName(filePath), fileDirectory);

        var reader = new CsvFileReader(filePath);
        
        Console.WriteLine($"Detected encoding: {reader.Encoding.EncodingName}");
        
        var csvData = await reader.ReadAsync();

        if (HandleEmptyFile(csvData))
            return;

        Console.WriteLine($"Total lines in file (including blank lines): {csvData.TotalLines:N0}");

        var writer = new CsvFileWriter(outputDirectory, fileName, reader.Encoding);

        if (linesPerFile == -1)
            await ProcessSingleFileMode(csvData, writer);
        else
            await ProcessMultiFileMode(csvData, writer, linesPerFile);

        DisplayCompletion(outputDirectory);
    }

    private static void ValidateFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");
    }

    private static void DisplayProcessingInfo(string fileName, string directory)
    {
        Console.WriteLine($"Loading CSV file: {fileName}...");
        Console.WriteLine($"Source directory: {directory}");
        Console.WriteLine("Note: All data will be treated as text to preserve formatting (e.g., leading zeros)");
    }

    private static bool HandleEmptyFile(CsvData data)
    {
        if (data.TotalLines == 0)
        {
            Console.WriteLine("The file is empty.");
            return true;
        }

        if (data.IsEmpty)
        {
            Console.WriteLine("The file contains only a header row (or blank lines).");
            return true;
        }

        return false;
    }

    private static async Task ProcessSingleFileMode(CsvData data, CsvFileWriter writer)
    {
        Console.WriteLine($"Data lines to process: {data.DataLines.Count:N0}");
        Console.WriteLine("Mode: Single file output (ignoring blank lines)\n");

        await writer.WriteCompleteFileAsync(data.Header, data.DataLines);
    }

    private static async Task ProcessMultiFileMode(CsvData data, CsvFileWriter writer, int linesPerFile)
    {
        var totalFiles = (int)Math.Ceiling((double)data.DataLines.Count / linesPerFile);
        
        Console.WriteLine($"Data lines to process: {data.DataLines.Count:N0}");
        Console.WriteLine($"Will create {totalFiles} output files\n");

        var partNumber = 1;
        var startIndex = 0;

        while (startIndex < data.DataLines.Count)
        {
            var endIndex = Math.Min(startIndex + linesPerFile - 1, data.DataLines.Count - 1);
            var partLines = data.DataLines.GetRange(startIndex, endIndex - startIndex + 1);

            await writer.WritePartFileAsync(data.Header, partLines, partNumber, totalFiles);

            partNumber++;
            startIndex += linesPerFile;
        }
    }

    private static void DisplayCompletion(string outputDirectory)
    {
        Console.WriteLine("\nCSV processing completed successfully!");
        Console.WriteLine($"Output directory: {outputDirectory}");
    }
}
