using FileSplitter.Features.Csv.Validators;
using FileSplitter.Infrastructure.Encoding;
using System.Text;

namespace FileSplitter.Features.Csv;

public sealed class CsvFileReader
{
    private readonly string _filePath;
    private readonly System.Text.Encoding _encoding;

    public CsvFileReader(string filePath)
    {
        _filePath = filePath;
        _encoding = EncodingDetector.Detect(filePath);
    }

    public System.Text.Encoding Encoding => _encoding;

    public async Task<CsvData> ReadAsync()
    {
        var allLines = await File.ReadAllLinesAsync(_filePath, _encoding);

        if (allLines.Length == 0)
            return CsvData.Empty();

        var header = allLines[0];
        var dataLines = FilterDataLines(allLines);

        return new CsvData(header, dataLines, allLines.Length);
    }

    private static List<string> FilterDataLines(string[] allLines)
    {
        var dataLines = new List<string>();
        var filteredCount = 0;

        for (int i = 1; i < allLines.Length; i++)
        {
            var line = allLines[i];
            
            if (!CsvLineValidator.IsEmpty(line))
            {
                dataLines.Add(line);
            }
            else
            {
                filteredCount++;
                Console.WriteLine($"  [Filtered] Line {i + 1}: Empty or only delimiters");
            }
        }

        if (filteredCount > 0)
            Console.WriteLine($"Ignored {filteredCount} blank line(s)");

        return dataLines;
    }
}

public sealed class CsvData
{
    public string Header { get; }
    public List<string> DataLines { get; }
    public int TotalLines { get; }
    public bool IsEmpty => DataLines.Count == 0;

    public CsvData(string header, List<string> dataLines, int totalLines)
    {
        Header = header;
        DataLines = dataLines;
        TotalLines = totalLines;
    }

    public static CsvData Empty() => new(string.Empty, new List<string>(), 0);
}
