using ClosedXML.Excel;

namespace FileSplitter.Features.Excel;

public static class ExcelRowCopier
{
    public static void CopyRow(IXLRow sourceRow, IXLRow targetRow)
    {
        foreach (var cell in sourceRow.CellsUsed())
        {
            var targetCell = targetRow.Cell(cell.Address.ColumnNumber);
            var cellValue = cell.GetFormattedString();
            
            targetCell.Value = cellValue;
            targetCell.Style = cell.Style;
            targetCell.Style.NumberFormat.Format = "@";
        }
    }
}
