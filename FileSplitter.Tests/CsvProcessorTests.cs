using FileSplitter.Tests.TestData;
using FileSplitter.Features.Csv;
using System.Text;

namespace FileSplitter.Tests
{
    public class CsvProcessorTests : TestBase
    {
        private readonly CsvProcessor _processor = new();

        #region Basic Functionality Tests

        [Fact]
        public async Task ProcessFileAsync_WithValidFile_SplitsIntoMultipleFiles()
        {
            var testFile = GetTestFilePath("test.csv");
            await TestFileGenerator.CreateTestCsvFileAsync(testFile, dataRows: 100);
            int linesPerFile = 30;

            await _processor.ProcessAsync(testFile, linesPerFile);

            var outputDirectory = Path.Combine(TestDirectory, "output_test");
            TrackDirectory(outputDirectory);
            
            Assert.True(Directory.Exists(outputDirectory));
            
            var outputFiles = Directory.GetFiles(outputDirectory, "*.csv");
            Assert.Equal(4, outputFiles.Length);
            
            Assert.Contains(Path.Combine(outputDirectory, "test_part_001.csv"), outputFiles);
            Assert.Contains(Path.Combine(outputDirectory, "test_part_002.csv"), outputFiles);
            Assert.Contains(Path.Combine(outputDirectory, "test_part_003.csv"), outputFiles);
            Assert.Contains(Path.Combine(outputDirectory, "test_part_004.csv"), outputFiles);
        }

        [Fact]
        public async Task ProcessFileAsync_WithValidFile_PreservesHeaderInEachFile()
        {
            var testFile = GetTestFilePath("test.csv");
            await TestFileGenerator.CreateTestCsvFileAsync(testFile, dataRows: 50);
            int linesPerFile = 20;

            await _processor.ProcessAsync(testFile, linesPerFile);

            var outputDirectory = Path.Combine(TestDirectory, "output_test");
            TrackDirectory(outputDirectory);
            
            var outputFiles = Directory.GetFiles(outputDirectory, "*.csv");
            
            foreach (var file in outputFiles)
            {
                var lines = await File.ReadAllLinesAsync(file);
                Assert.True(lines.Length > 0);
                Assert.Equal("ID,Name,Email,Age,City", lines[0]);
            }
        }

        [Fact]
        public async Task ProcessFileAsync_WithValidFile_SplitsDataCorrectly()
        {
            var testFile = GetTestFilePath("test.csv");
            await TestFileGenerator.CreateTestCsvFileAsync(testFile, dataRows: 50);
            int linesPerFile = 20;

            await _processor.ProcessAsync(testFile, linesPerFile);

            var outputDirectory = Path.Combine(TestDirectory, "output_test");
            TrackDirectory(outputDirectory);
            
            var outputFiles = Directory.GetFiles(outputDirectory, "*.csv").OrderBy(f => f).ToArray();
            
            var file1Lines = await File.ReadAllLinesAsync(outputFiles[0]);
            Assert.Equal(21, file1Lines.Length);
            
            var file2Lines = await File.ReadAllLinesAsync(outputFiles[1]);
            Assert.Equal(21, file2Lines.Length);
            
            var file3Lines = await File.ReadAllLinesAsync(outputFiles[2]);
            Assert.Equal(11, file3Lines.Length);
        }

        #endregion

        #region Encoding Detection Tests

        [Fact]
        public async Task ProcessFileAsync_WithUtf8BomEncoding_DetectsAndPreservesEncoding()
        {
            var testFile = GetTestFilePath("test_bom.csv");
            await TestFileGenerator.CreateCsvFileWithBomAsync(testFile, dataRows: 10);
            int linesPerFile = 5;

            await _processor.ProcessAsync(testFile, linesPerFile);

            var outputDirectory = Path.Combine(TestDirectory, "output_test_bom");
            TrackDirectory(outputDirectory);
            
            var outputFiles = Directory.GetFiles(outputDirectory, "*.csv");
            Assert.NotEmpty(outputFiles);
            
            var content = await File.ReadAllTextAsync(outputFiles[0]);
            Assert.Contains("ID,Name,Email,Age,City", content);
        }

        [Fact]
        public async Task ProcessFileAsync_WithUnicodeEncoding_DetectsAndPreservesEncoding()
        {
            var testFile = GetTestFilePath("test_unicode.csv");
            await TestFileGenerator.CreateCsvFileWithUnicodeAsync(testFile, dataRows: 10);
            int linesPerFile = 5;

            await _processor.ProcessAsync(testFile, linesPerFile);

            var outputDirectory = Path.Combine(TestDirectory, "output_test_unicode");
            TrackDirectory(outputDirectory);
            
            var outputFiles = Directory.GetFiles(outputDirectory, "*.csv");
            Assert.NotEmpty(outputFiles);
            
            var content = await File.ReadAllTextAsync(outputFiles[0], Encoding.Unicode);
            Assert.Contains("ID,Name,Email,Age,City", content);
        }

        #endregion

        #region Edge Cases Tests

        [Fact]
        public async Task ProcessFileAsync_WithEmptyFile_HandlesGracefully()
        {
            var testFile = GetTestFilePath("empty.csv");
            await TestFileGenerator.CreateBlankCsvFileAsync(testFile);
            int linesPerFile = 10;

            await _processor.ProcessAsync(testFile, linesPerFile);

            var outputDirectory = Path.Combine(TestDirectory, "output_empty");
            
            Assert.True(Directory.Exists(outputDirectory));
            var outputFiles = Directory.GetFiles(outputDirectory, "*.csv");
            Assert.Empty(outputFiles);
        }

        [Fact]
        public async Task ProcessFileAsync_WithOnlyHeader_HandlesGracefully()
        {
            var testFile = GetTestFilePath("header_only.csv");
            await TestFileGenerator.CreateEmptyCsvFileAsync(testFile);
            int linesPerFile = 10;

            await _processor.ProcessAsync(testFile, linesPerFile);

            var outputDirectory = Path.Combine(TestDirectory, "output_header_only");
            
            Assert.True(Directory.Exists(outputDirectory));
            var outputFiles = Directory.GetFiles(outputDirectory, "*.csv");
            Assert.Empty(outputFiles);
        }

        [Fact]
        public async Task ProcessFileAsync_WithOneDataRow_CreatesOneOutputFile()
        {
            var testFile = GetTestFilePath("single_row.csv");
            await TestFileGenerator.CreateTestCsvFileAsync(testFile, dataRows: 1);
            int linesPerFile = 10;

            await _processor.ProcessAsync(testFile, linesPerFile);

            var outputDirectory = Path.Combine(TestDirectory, "output_single_row");
            TrackDirectory(outputDirectory);
            
            var outputFiles = Directory.GetFiles(outputDirectory, "*.csv");
            Assert.Single(outputFiles);
            
            var lines = await File.ReadAllLinesAsync(outputFiles[0]);
            Assert.Equal(2, lines.Length);
        }

        [Fact]
        public async Task ProcessFileAsync_WhenLinesPerFileEqualsDataRows_CreatesOneFile()
        {
            var testFile = GetTestFilePath("exact_match.csv");
            await TestFileGenerator.CreateTestCsvFileAsync(testFile, dataRows: 25);
            int linesPerFile = 25;

            await _processor.ProcessAsync(testFile, linesPerFile);

            var outputDirectory = Path.Combine(TestDirectory, "output_exact_match");
            TrackDirectory(outputDirectory);
            
            var outputFiles = Directory.GetFiles(outputDirectory, "*.csv");
            Assert.Single(outputFiles);
            
            var lines = await File.ReadAllLinesAsync(outputFiles[0]);
            Assert.Equal(26, lines.Length);
        }

        [Fact]
        public async Task ProcessFileAsync_WhenLinesPerFileExceedsDataRows_CreatesOneFile()
        {
            var testFile = GetTestFilePath("small_file.csv");
            await TestFileGenerator.CreateTestCsvFileAsync(testFile, dataRows: 10);
            int linesPerFile = 100;

            await _processor.ProcessAsync(testFile, linesPerFile);

            var outputDirectory = Path.Combine(TestDirectory, "output_small_file");
            TrackDirectory(outputDirectory);
            
            var outputFiles = Directory.GetFiles(outputDirectory, "*.csv");
            Assert.Single(outputFiles);
            
            var lines = await File.ReadAllLinesAsync(outputFiles[0]);
            Assert.Equal(11, lines.Length);
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public async Task ProcessFileAsync_WithNonExistentFile_ThrowsFileNotFoundException()
        {
            var nonExistentFile = Path.Combine(TestDirectory, "nonexistent.csv");
            int linesPerFile = 10;

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
            var testFile = GetTestFilePath("mydatafile.csv");
            await TestFileGenerator.CreateTestCsvFileAsync(testFile, dataRows: 20);
            int linesPerFile = 10;

            await _processor.ProcessAsync(testFile, linesPerFile);

            var expectedOutputDirectory = Path.Combine(TestDirectory, "output_mydatafile");
            TrackDirectory(expectedOutputDirectory);
            
            Assert.True(Directory.Exists(expectedOutputDirectory));
        }

        [Fact]
        public async Task ProcessFileAsync_WhenOutputDirectoryExists_UsesExistingDirectory()
        {
            var testFile = GetTestFilePath("existing_dir.csv");
            await TestFileGenerator.CreateTestCsvFileAsync(testFile, dataRows: 15);
            
            var outputDirectory = Path.Combine(TestDirectory, "output_existing_dir");
            Directory.CreateDirectory(outputDirectory);
            TrackDirectory(outputDirectory);
            
            int linesPerFile = 10;

            await _processor.ProcessAsync(testFile, linesPerFile);

            Assert.True(Directory.Exists(outputDirectory));
            var outputFiles = Directory.GetFiles(outputDirectory, "*.csv");
            Assert.Equal(2, outputFiles.Length);
        }

        #endregion

        #region Large File Tests

        [Fact]
        public async Task ProcessFileAsync_WithLargeFile_SplitsCorrectly()
        {
            var testFile = GetTestFilePath("large_file.csv");
            await TestFileGenerator.CreateTestCsvFileAsync(testFile, dataRows: 1000);
            int linesPerFile = 100;

            await _processor.ProcessAsync(testFile, linesPerFile);

            var outputDirectory = Path.Combine(TestDirectory, "output_large_file");
            TrackDirectory(outputDirectory);
            
            var outputFiles = Directory.GetFiles(outputDirectory, "*.csv").OrderBy(f => f).ToArray();
            Assert.Equal(10, outputFiles.Length);
            
            for (int i = 0; i < outputFiles.Length; i++)
            {
                var lines = await File.ReadAllLinesAsync(outputFiles[i], TestContext.Current.CancellationToken);
                Assert.Equal(101, lines.Length);
            }
        }

        #endregion
    }
}

