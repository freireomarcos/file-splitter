using ClosedXML.Excel;
using FileSplitter.Tests.TestData;
using FileSplitter.Features.Excel;

namespace FileSplitter.Tests
{
    public class ExcelProcessorTests : TestBase
    {
        private readonly ExcelProcessor _processor = new();
        #region Basic Functionality Tests

        [Fact]
        public async Task ProcessFileAsync_WithValidFile_SplitsIntoMultipleFiles()
        {
            // Arrange
            var testFile = GetTestFilePath("test.xlsx");
            TestFileGenerator.CreateTestExcelFile(testFile, dataRows: 100);
            int linesPerFile = 30;

            // Act
            await _processor.ProcessAsync(testFile, linesPerFile);

            // Assert
            var outputDirectory = Path.Combine(TestDirectory, "output_test");
            TrackDirectory(outputDirectory);
            
            Assert.True(Directory.Exists(outputDirectory));
            
            var outputFiles = Directory.GetFiles(outputDirectory, "*.xlsx");
            Assert.Equal(4, outputFiles.Length); // 100 rows / 30 per file = 4 files
            
            // Verify file names
            Assert.Contains(Path.Combine(outputDirectory, "test_part_001.xlsx"), outputFiles);
            Assert.Contains(Path.Combine(outputDirectory, "test_part_002.xlsx"), outputFiles);
            Assert.Contains(Path.Combine(outputDirectory, "test_part_003.xlsx"), outputFiles);
            Assert.Contains(Path.Combine(outputDirectory, "test_part_004.xlsx"), outputFiles);
        }

        [Fact]
        public async Task ProcessFileAsync_WithValidFile_PreservesHeaderInEachFile()
        {
            // Arrange
            var testFile = GetTestFilePath("test.xlsx");
            TestFileGenerator.CreateTestExcelFile(testFile, dataRows: 50);
            int linesPerFile = 20;

            // Act
            await _processor.ProcessAsync(testFile, linesPerFile);

            // Assert
            var outputDirectory = Path.Combine(TestDirectory, "output_test");
            TrackDirectory(outputDirectory);
            
            var outputFiles = Directory.GetFiles(outputDirectory, "*.xlsx");
            
            foreach (var file in outputFiles)
            {
                using var workbook = new XLWorkbook(file);
                var worksheet = workbook.Worksheet(1);
                
                Assert.Equal("ID", worksheet.Cell(1, 1).Value.ToString());
                Assert.Equal("Name", worksheet.Cell(1, 2).Value.ToString());
                Assert.Equal("Email", worksheet.Cell(1, 3).Value.ToString());
                Assert.Equal("Age", worksheet.Cell(1, 4).Value.ToString());
                Assert.Equal("City", worksheet.Cell(1, 5).Value.ToString());
            }
        }

        [Fact]
        public async Task ProcessFileAsync_WithValidFile_SplitsDataCorrectly()
        {
            // Arrange
            var testFile = GetTestFilePath("test.xlsx");
            TestFileGenerator.CreateTestExcelFile(testFile, dataRows: 50);
            int linesPerFile = 20;

            // Act
            await _processor.ProcessAsync(testFile, linesPerFile);

            // Assert
            var outputDirectory = Path.Combine(TestDirectory, "output_test");
            TrackDirectory(outputDirectory);
            
            var outputFiles = Directory.GetFiles(outputDirectory, "*.xlsx").OrderBy(f => f).ToArray();
            
            // First file: header + 20 data rows = 21 rows
            using (var workbook = new XLWorkbook(outputFiles[0]))
            {
                var worksheet = workbook.Worksheet(1);
                Assert.Equal(21, worksheet.LastRowUsed()?.RowNumber() ?? 0);
            }
            
            // Second file: header + 20 data rows = 21 rows
            using (var workbook = new XLWorkbook(outputFiles[1]))
            {
                var worksheet = workbook.Worksheet(1);
                Assert.Equal(21, worksheet.LastRowUsed()?.RowNumber() ?? 0);
            }
            
            // Third file: header + 10 data rows = 11 rows (remaining rows)
            using (var workbook = new XLWorkbook(outputFiles[2]))
            {
                var worksheet = workbook.Worksheet(1);
                Assert.Equal(11, worksheet.LastRowUsed()?.RowNumber() ?? 0);
            }
        }

        [Fact]
        public async Task ProcessFileAsync_PreservesDataIntegrity()
        {
            // Arrange
            var testFile = GetTestFilePath("data_integrity.xlsx");
            TestFileGenerator.CreateTestExcelFile(testFile, dataRows: 30);
            int linesPerFile = 10;

            // Act
            await _processor.ProcessAsync(testFile, linesPerFile);

            // Assert
            var outputDirectory = Path.Combine(TestDirectory, "output_data_integrity");
            TrackDirectory(outputDirectory);
            
            var outputFiles = Directory.GetFiles(outputDirectory, "*.xlsx").OrderBy(f => f).ToArray();
            
            // Verify first file has correct data
            using (var workbook = new XLWorkbook(outputFiles[0]))
            {
                var worksheet = workbook.Worksheet(1);
                
                // Check first data row (row 2)
                Assert.Equal("1", worksheet.Cell(2, 1).Value.ToString());
                Assert.Equal("User1", worksheet.Cell(2, 2).Value.ToString());
                Assert.Equal("user1@example.com", worksheet.Cell(2, 3).Value.ToString());
                
                // Check last data row in first file (row 11 = header + 10 rows)
                Assert.Equal("10", worksheet.Cell(11, 1).Value.ToString());
                Assert.Equal("User10", worksheet.Cell(11, 2).Value.ToString());
            }
            
            // Verify second file starts with correct data
            using (var workbook = new XLWorkbook(outputFiles[1]))
            {
                var worksheet = workbook.Worksheet(1);
                
                // Check first data row (row 2)
                Assert.Equal("11", worksheet.Cell(2, 1).Value.ToString());
                Assert.Equal("User11", worksheet.Cell(2, 2).Value.ToString());
            }
        }

        #endregion

        #region Formatting Preservation Tests

        [Fact]
        public async Task ProcessFileAsync_PreservesHeaderFormatting()
        {
            // Arrange
            var testFile = GetTestFilePath("formatting.xlsx");
            TestFileGenerator.CreateTestExcelFile(testFile, dataRows: 25);
            int linesPerFile = 10;

            // Act
            await _processor.ProcessAsync(testFile, linesPerFile);

            // Assert
            var outputDirectory = Path.Combine(TestDirectory, "output_formatting");
            TrackDirectory(outputDirectory);
            
            var outputFiles = Directory.GetFiles(outputDirectory, "*.xlsx");
            
            foreach (var file in outputFiles)
            {
                using var workbook = new XLWorkbook(file);
                var worksheet = workbook.Worksheet(1);
                var headerCell = worksheet.Cell(1, 1);
                
                // Verify header formatting is preserved
                Assert.True(headerCell.Style.Font.Bold);
                Assert.Equal(XLColor.LightGray, headerCell.Style.Fill.BackgroundColor);
            }
        }

        #endregion

        #region Edge Cases Tests

        [Fact]
        public async Task ProcessFileAsync_WithEmptyFile_HandlesGracefully()
        {
            // Arrange
            var testFile = GetTestFilePath("empty.xlsx");
            
            using (var workbook = new XLWorkbook())
            {
                workbook.Worksheets.Add("Sheet1");
                workbook.SaveAs(testFile);
            }
            
            int linesPerFile = 10;

            // Act
            await _processor.ProcessAsync(testFile, linesPerFile);

            // Assert
            var outputDirectory = Path.Combine(TestDirectory, "output_empty");
            
            // Directory should be created but no files should be generated
            Assert.True(Directory.Exists(outputDirectory));
            var outputFiles = Directory.GetFiles(outputDirectory, "*.xlsx");
            Assert.Empty(outputFiles);
        }

        [Fact]
        public async Task ProcessFileAsync_WithOnlyHeader_HandlesGracefully()
        {
            // Arrange
            var testFile = GetTestFilePath("header_only.xlsx");
            TestFileGenerator.CreateEmptyExcelFile(testFile);
            int linesPerFile = 10;

            // Act
            await _processor.ProcessAsync(testFile, linesPerFile);

            // Assert
            var outputDirectory = Path.Combine(TestDirectory, "output_header_only");
            
            // Directory should be created but no files should be generated
            Assert.True(Directory.Exists(outputDirectory));
            var outputFiles = Directory.GetFiles(outputDirectory, "*.xlsx");
            Assert.Empty(outputFiles);
        }

        [Fact]
        public async Task ProcessFileAsync_WithOneDataRow_CreatesOneOutputFile()
        {
            // Arrange
            var testFile = GetTestFilePath("single_row.xlsx");
            TestFileGenerator.CreateTestExcelFile(testFile, dataRows: 1);
            int linesPerFile = 10;

            // Act
            await _processor.ProcessAsync(testFile, linesPerFile);

            // Assert
            var outputDirectory = Path.Combine(TestDirectory, "output_single_row");
            TrackDirectory(outputDirectory);
            
            var outputFiles = Directory.GetFiles(outputDirectory, "*.xlsx");
            Assert.Single(outputFiles);
            
            using var workbook = new XLWorkbook(outputFiles[0]);
            var worksheet = workbook.Worksheet(1);
            Assert.Equal(2, worksheet.LastRowUsed()?.RowNumber() ?? 0); // Header + 1 data row
        }

        [Fact]
        public async Task ProcessFileAsync_WhenLinesPerFileEqualsDataRows_CreatesOneFile()
        {
            // Arrange
            var testFile = GetTestFilePath("exact_match.xlsx");
            TestFileGenerator.CreateTestExcelFile(testFile, dataRows: 25);
            int linesPerFile = 25;

            // Act
            await _processor.ProcessAsync(testFile, linesPerFile);

            // Assert
            var outputDirectory = Path.Combine(TestDirectory, "output_exact_match");
            TrackDirectory(outputDirectory);
            
            var outputFiles = Directory.GetFiles(outputDirectory, "*.xlsx");
            Assert.Single(outputFiles);
            
            using var workbook = new XLWorkbook(outputFiles[0]);
            var worksheet = workbook.Worksheet(1);
            Assert.Equal(26, worksheet.LastRowUsed()?.RowNumber() ?? 0); // Header + 25 data rows
        }

        [Fact]
        public async Task ProcessFileAsync_WhenLinesPerFileExceedsDataRows_CreatesOneFile()
        {
            // Arrange
            var testFile = GetTestFilePath("small_file.xlsx");
            TestFileGenerator.CreateTestExcelFile(testFile, dataRows: 10);
            int linesPerFile = 100;

            // Act
            await _processor.ProcessAsync(testFile, linesPerFile);

            // Assert
            var outputDirectory = Path.Combine(TestDirectory, "output_small_file");
            TrackDirectory(outputDirectory);
            
            var outputFiles = Directory.GetFiles(outputDirectory, "*.xlsx");
            Assert.Single(outputFiles);
            
            using var workbook = new XLWorkbook(outputFiles[0]);
            var worksheet = workbook.Worksheet(1);
            Assert.Equal(11, worksheet.LastRowUsed()?.RowNumber() ?? 0); // Header + 10 data rows
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public async Task ProcessFileAsync_WithNonExistentFile_ThrowsFileNotFoundException()
        {
            // Arrange
            var nonExistentFile = Path.Combine(TestDirectory, "nonexistent.xlsx");
            int linesPerFile = 10;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<FileNotFoundException>(
                () => _processor.ProcessAsync(nonExistentFile, linesPerFile)
            );
            
            Assert.Contains("File not found", exception.Message);
        }

        #endregion

        #region Output Directory Tests

        [Fact]
        public async Task ProcessFileAsync_CreatesOutputDirectoryWithCorrectName()
        {
            // Arrange
            var testFile = GetTestFilePath("mydatafile.xlsx");
            TestFileGenerator.CreateTestExcelFile(testFile, dataRows: 20);
            int linesPerFile = 10;

            // Act
            await _processor.ProcessAsync(testFile, linesPerFile);

            // Assert
            var expectedOutputDirectory = Path.Combine(TestDirectory, "output_mydatafile");
            TrackDirectory(expectedOutputDirectory);
            
            Assert.True(Directory.Exists(expectedOutputDirectory));
        }

        [Fact]
        public async Task ProcessFileAsync_WhenOutputDirectoryExists_UsesExistingDirectory()
        {
            // Arrange
            var testFile = GetTestFilePath("existing_dir.xlsx");
            TestFileGenerator.CreateTestExcelFile(testFile, dataRows: 15);
            
            var outputDirectory = Path.Combine(TestDirectory, "output_existing_dir");
            Directory.CreateDirectory(outputDirectory);
            TrackDirectory(outputDirectory);
            
            int linesPerFile = 10;

            // Act
            await _processor.ProcessAsync(testFile, linesPerFile);

            // Assert
            Assert.True(Directory.Exists(outputDirectory));
            var outputFiles = Directory.GetFiles(outputDirectory, "*.xlsx");
            Assert.Equal(2, outputFiles.Length);
        }

        #endregion

        #region Large File Tests

        [Fact]
        public async Task ProcessFileAsync_WithLargeFile_SplitsCorrectly()
        {
            // Arrange
            var testFile = GetTestFilePath("large_file.xlsx");
            TestFileGenerator.CreateTestExcelFile(testFile, dataRows: 500);
            int linesPerFile = 100;

            // Act
            await _processor.ProcessAsync(testFile, linesPerFile);

            // Assert
            var outputDirectory = Path.Combine(TestDirectory, "output_large_file");
            TrackDirectory(outputDirectory);
            
            var outputFiles = Directory.GetFiles(outputDirectory, "*.xlsx").OrderBy(f => f).ToArray();
            Assert.Equal(5, outputFiles.Length); // 500 / 100 = 5 files
            
            // Verify each file has correct number of rows (header + 100 data rows)
            for (int i = 0; i < outputFiles.Length; i++)
            {
                using var workbook = new XLWorkbook(outputFiles[i]);
                var worksheet = workbook.Worksheet(1);
                Assert.Equal(101, worksheet.LastRowUsed()?.RowNumber() ?? 0);
            }
        }

        #endregion

        #region Worksheet Name Tests

        [Fact]
        public async Task ProcessFileAsync_PreservesWorksheetName()
        {
            // Arrange
            var testFile = GetTestFilePath("worksheet_name.xlsx");
            TestFileGenerator.CreateTestExcelFile(testFile, dataRows: 20);
            int linesPerFile = 10;

            // Act
            await _processor.ProcessAsync(testFile, linesPerFile);

            // Assert
            var outputDirectory = Path.Combine(TestDirectory, "output_worksheet_name");
            TrackDirectory(outputDirectory);
            
            var outputFiles = Directory.GetFiles(outputDirectory, "*.xlsx");
            
            foreach (var file in outputFiles)
            {
                using var workbook = new XLWorkbook(file);
                var worksheet = workbook.Worksheet(1);
                Assert.Equal("TestData", worksheet.Name);
            }
        }

        #endregion
    }
}
