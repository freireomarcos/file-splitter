using FileSplitter.Application.Contracts;

namespace FileSplitter.Features.Excel;

public sealed class ExcelProcessor : IFileProcessor
{
    public async Task ProcessAsync(string filePath, int linesPerFile)
    {
        ValidateFile(filePath);

        var fileDirectory = Path.GetDirectoryName(filePath) ?? "";
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var outputDirectory = Path.Combine(fileDirectory, $"output_{fileName}");

        DisplayProcessingInfo(Path.GetFileName(filePath), fileDirectory);

        await Task.Run(() => ProcessExcelFile(filePath, outputDirectory, fileName, linesPerFile));
    }

    private static void ValidateFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");
    }

    private static void DisplayProcessingInfo(string fileName, string directory)
    {
        Console.WriteLine($"Loading Excel file: {fileName}...");
        Console.WriteLine($"Source directory: {directory}");
        Console.WriteLine("Note: All cells will be treated as text to preserve formatting (e.g., leading zeros)");
    }

    private static void ProcessExcelFile(string filePath, string outputDirectory, string fileName, int linesPerFile)
    {
        var reader = new ExcelFileReader(filePath);
        var excelData = reader.Read();

        if (HandleEmptyFile(excelData))
            return;

        DisplayDataInfo(excelData);

        var writer = new ExcelFileWriter(outputDirectory, fileName, excelData.WorksheetName);

        if (linesPerFile == -1)
            ProcessSingleFileMode(excelData, writer);
        else
            ProcessMultiFileMode(excelData, writer, linesPerFile);

        DisplayCompletion(outputDirectory);
    }

    private static void DisplayDataInfo(ExcelData data)
    {
        Console.WriteLine($"Total rows in file (including blank rows): {data.TotalRows:N0}");
    }

    private static bool HandleEmptyFile(ExcelData data)
    {
        if (data.TotalRows == 0)
        {
            Console.WriteLine("The Excel file appears to be empty.");
            return true;
        }

        if (data.IsEmpty)
        {
            Console.WriteLine("The file contains only a header row (or blank rows).");
            return true;
        }

        return false;
    }

    private static void ProcessSingleFileMode(ExcelData data, ExcelFileWriter writer)
    {
        Console.WriteLine($"Data rows to process: {data.DataRows.Count:N0}");
        Console.WriteLine("Mode: Single file output (ignoring blank rows)\n");

        writer.WriteCompleteFile(data.HeaderRow, data.DataRows);
    }

    private static void ProcessMultiFileMode(ExcelData data, ExcelFileWriter writer, int linesPerFile)
    {
        var totalFiles = (int)Math.Ceiling((double)data.DataRows.Count / linesPerFile);

        Console.WriteLine($"Data rows to process: {data.DataRows.Count:N0}");
        Console.WriteLine($"Will create {totalFiles} output files\n");

        var partNumber = 1;
        var startIndex = 0;

        while (startIndex < data.DataRows.Count)
        {
            var endIndex = Math.Min(startIndex + linesPerFile - 1, data.DataRows.Count - 1);
            var partRows = data.DataRows.GetRange(startIndex, endIndex - startIndex + 1);

            writer.WritePartFile(data.HeaderRow, partRows, partNumber, totalFiles);

            partNumber++;
            startIndex += linesPerFile;
        }
    }

    private static void DisplayCompletion(string outputDirectory)
    {
        Console.WriteLine("\nExcel processing completed successfully!");
        Console.WriteLine($"Output directory: {outputDirectory}");
    }
}
