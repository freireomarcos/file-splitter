using ClosedXML.Excel;

namespace FileSplitter.Features.Excel.Validators;

public static class ExcelRowValidator
{
    public static bool HasContent(IXLRow row)
    {
        var cellsUsed = row.CellsUsed();
        
        if (!cellsUsed.Any())
            return false;

        foreach (var cell in cellsUsed)
        {
            var cellValue = cell.GetFormattedString();
            if (!string.IsNullOrWhiteSpace(cellValue))
                return true;
        }

        return false;
    }
}
