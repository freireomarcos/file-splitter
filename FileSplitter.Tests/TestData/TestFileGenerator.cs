using ClosedXML.Excel;
using System.Text;

namespace FileSplitter.Tests.TestData
{
    /// <summary>
    /// Helper class to generate test files for CSV and Excel processing tests.
    /// </summary>
    public static class TestFileGenerator
    {
        /// <summary>
        /// Creates a test CSV file with the specified number of data rows.
        /// </summary>
        /// <param name="filePath">The path where the CSV file will be created.</param>
        /// <param name="dataRows">Number of data rows to generate (excluding header).</param>
        /// <param name="encoding">The encoding to use for the file. Defaults to UTF8.</param>
        public static async Task CreateTestCsvFileAsync(string filePath, int dataRows, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            
            var lines = new List<string>
            {
                "ID,Name,Email,Age,City"
            };

            for (int i = 1; i <= dataRows; i++)
            {
                lines.Add($"{i},User{i},user{i}@example.com,{20 + (i % 50)},City{i % 10}");
            }

            await File.WriteAllLinesAsync(filePath, lines, encoding);
        }

        /// <summary>
        /// Creates a test Excel file with the specified number of data rows.
        /// </summary>
        /// <param name="filePath">The path where the Excel file will be created.</param>
        /// <param name="dataRows">Number of data rows to generate (excluding header).</param>
        public static void CreateTestExcelFile(string filePath, int dataRows)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("TestData");

            // Add header
            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Name";
            worksheet.Cell(1, 3).Value = "Email";
            worksheet.Cell(1, 4).Value = "Age";
            worksheet.Cell(1, 5).Value = "City";

            // Style header
            var headerRange = worksheet.Range(1, 1, 1, 5);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            // Add data rows
            for (int i = 1; i <= dataRows; i++)
            {
                int row = i + 1;
                worksheet.Cell(row, 1).Value = i;
                worksheet.Cell(row, 2).Value = $"User{i}";
                worksheet.Cell(row, 3).Value = $"user{i}@example.com";
                worksheet.Cell(row, 4).Value = 20 + (i % 50);
                worksheet.Cell(row, 5).Value = $"City{i % 10}";
            }

            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(filePath);
        }

        /// <summary>
        /// Creates an empty CSV file with only a header.
        /// </summary>
        public static async Task CreateEmptyCsvFileAsync(string filePath)
        {
            await File.WriteAllTextAsync(filePath, "ID,Name,Email,Age,City");
        }

        /// <summary>
        /// Creates an empty Excel file with only a header.
        /// </summary>
        public static void CreateEmptyExcelFile(string filePath)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("TestData");

            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Name";
            worksheet.Cell(1, 3).Value = "Email";
            worksheet.Cell(1, 4).Value = "Age";
            worksheet.Cell(1, 5).Value = "City";

            workbook.SaveAs(filePath);
        }

        /// <summary>
        /// Creates a completely empty CSV file.
        /// </summary>
        public static async Task CreateBlankCsvFileAsync(string filePath)
        {
            await File.WriteAllTextAsync(filePath, string.Empty);
        }

        /// <summary>
        /// Creates a CSV file with UTF-8 BOM encoding.
        /// </summary>
        public static async Task CreateCsvFileWithBomAsync(string filePath, int dataRows)
        {
            await CreateTestCsvFileAsync(filePath, dataRows, new UTF8Encoding(true));
        }

        /// <summary>
        /// Creates a CSV file with Unicode encoding.
        /// </summary>
        public static async Task CreateCsvFileWithUnicodeAsync(string filePath, int dataRows)
        {
            await CreateTestCsvFileAsync(filePath, dataRows, Encoding.Unicode);
        }
    }
}
