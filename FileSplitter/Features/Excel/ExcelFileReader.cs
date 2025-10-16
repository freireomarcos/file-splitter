using ClosedXML.Excel;
using FileSplitter.Features.Excel.Validators;

namespace FileSplitter.Features.Excel;

public sealed class ExcelFileReader
{
    private readonly string _filePath;

    public ExcelFileReader(string filePath)
    {
        _filePath = filePath;
    }

    public ExcelData Read()
    {
        using var workbook = new XLWorkbook(_filePath);
        var worksheet = workbook.Worksheet(1);

        var lastRowUsed = worksheet.LastRowUsed();
        if (lastRowUsed == null)
            return ExcelData.Empty();

        var totalRows = lastRowUsed.RowNumber();

        if (totalRows <= 1)
            return ExcelData.Empty();

        var headerRow = worksheet.Row(1);
        var dataRows = FilterDataRows(worksheet, totalRows);

        return new ExcelData(worksheet.Name, headerRow, dataRows, totalRows);
    }

    private static List<IXLRow> FilterDataRows(IXLWorksheet worksheet, int totalRows)
    {
        var dataRows = new List<IXLRow>();
        var filteredCount = 0;

        for (int i = 2; i <= totalRows; i++)
        {
            var row = worksheet.Row(i);

            if (ExcelRowValidator.HasContent(row))
            {
                dataRows.Add(row);
            }
            else
            {
                filteredCount++;
                Console.WriteLine($"  [Filtered] Row {i}: Empty or only whitespace");
            }
        }

        if (filteredCount > 0)
            Console.WriteLine($"Ignored {filteredCount} blank row(s)");

        return dataRows;
    }
}

public sealed class ExcelData
{
    public string WorksheetName { get; }
    public IXLRow HeaderRow { get; }
    public List<IXLRow> DataRows { get; }
    public int TotalRows { get; }
    public bool IsEmpty => DataRows.Count == 0;

    public ExcelData(string worksheetName, IXLRow headerRow, List<IXLRow> dataRows, int totalRows)
    {
        WorksheetName = worksheetName;
        HeaderRow = headerRow;
        DataRows = dataRows;
        TotalRows = totalRows;
    }

    public static ExcelData Empty() => new(string.Empty, null!, new List<IXLRow>(), 0);
}
