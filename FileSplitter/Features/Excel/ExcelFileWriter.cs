using ClosedXML.Excel;

namespace FileSplitter.Features.Excel;

public sealed class ExcelFileWriter
{
    private readonly string _outputDirectory;
    private readonly string _fileName;
    private readonly string _worksheetName;

    public ExcelFileWriter(string outputDirectory, string fileName, string worksheetName)
    {
        _outputDirectory = outputDirectory;
        _fileName = fileName;
        _worksheetName = worksheetName;

        EnsureDirectoryExists();
    }

    public void WriteCompleteFile(IXLRow headerRow, List<IXLRow> dataRows)
    {
        var outputFile = Path.Combine(_outputDirectory, $"{_fileName}_complete.xlsx");

        Console.WriteLine($"Creating file: {_fileName}_complete.xlsx ({dataRows.Count:N0} data rows)");

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(_worksheetName);

        WriteRows(worksheet, headerRow, dataRows);
        
        worksheet.Columns().AdjustToContents();
        workbook.SaveAs(outputFile);
    }

    public void WritePartFile(IXLRow headerRow, List<IXLRow> dataRows, int partNumber, int totalParts)
    {
        var outputFile = Path.Combine(_outputDirectory, $"{_fileName}_part_{partNumber:D3}.xlsx");

        Console.WriteLine($"Creating file {partNumber}/{totalParts}: {_fileName}_part_{partNumber:D3}.xlsx ({dataRows.Count:N0} data rows)");

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(_worksheetName);

        WriteRows(worksheet, headerRow, dataRows);
        
        worksheet.Columns().AdjustToContents();
        workbook.SaveAs(outputFile);
    }

    private static void WriteRows(IXLWorksheet worksheet, IXLRow headerRow, List<IXLRow> dataRows)
    {
        ExcelRowCopier.CopyRow(headerRow, worksheet.Row(1));

        var rowIndex = 2;
        foreach (var dataRow in dataRows)
        {
            ExcelRowCopier.CopyRow(dataRow, worksheet.Row(rowIndex));
            rowIndex++;
        }
    }

    private void EnsureDirectoryExists()
    {
        if (!Directory.Exists(_outputDirectory))
            Directory.CreateDirectory(_outputDirectory);
    }
}
