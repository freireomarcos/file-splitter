using System.Text;

namespace FileSplitter.Features.Csv;

public sealed class CsvFileWriter
{
    private readonly string _outputDirectory;
    private readonly string _fileName;
    private readonly System.Text.Encoding _encoding;

    public CsvFileWriter(string outputDirectory, string fileName, System.Text.Encoding encoding)
    {
        _outputDirectory = outputDirectory;
        _fileName = fileName;
        _encoding = encoding;

        EnsureDirectoryExists();
    }

    public async Task WriteCompleteFileAsync(string header, List<string> dataLines)
    {
        var outputFile = Path.Combine(_outputDirectory, $"{_fileName}_complete.csv");
        var linesToWrite = BuildLines(header, dataLines);

        Console.WriteLine($"Creating file: {Path.GetFileName(outputFile)} ({dataLines.Count:N0} data lines)");
        
        await File.WriteAllLinesAsync(outputFile, linesToWrite, _encoding);
    }

    public async Task WritePartFileAsync(string header, List<string> dataLines, int partNumber, int totalParts)
    {
        var outputFile = Path.Combine(_outputDirectory, $"{_fileName}_part_{partNumber:D3}.csv");
        var linesToWrite = BuildLines(header, dataLines);

        Console.WriteLine($"Creating file {partNumber}/{totalParts}: {Path.GetFileName(outputFile)} ({dataLines.Count:N0} data lines)");
        
        await File.WriteAllLinesAsync(outputFile, linesToWrite, _encoding);
    }

    private static List<string> BuildLines(string header, List<string> dataLines)
    {
        var lines = new List<string> { header };
        lines.AddRange(dataLines);
        return lines;
    }

    private void EnsureDirectoryExists()
    {
        if (!Directory.Exists(_outputDirectory))
            Directory.CreateDirectory(_outputDirectory);
    }
}
