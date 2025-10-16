using FileSplitter.Application.Contracts;
using FileSplitter.Application.Services;
using FileSplitter.Domain;
using FileSplitter.Features.Csv;
using FileSplitter.Features.Excel;
using Xunit;

namespace FileSplitter.Tests.Unit;

public class FileProcessorFactoryTests
{
    [Fact]
    public void GetProcessor_WithCsvFileType_ReturnsCsvProcessor()
    {
        var processors = new IFileProcessor[]
        {
            new CsvProcessor(),
            new ExcelProcessor()
        };
        var factory = new FileProcessorFactory(processors);

        var result = factory.GetProcessor(FileType.Csv);

        Assert.IsType<CsvProcessor>(result);
    }

    [Fact]
    public void GetProcessor_WithExcelFileType_ReturnsExcelProcessor()
    {
        var processors = new IFileProcessor[]
        {
            new CsvProcessor(),
            new ExcelProcessor()
        };
        var factory = new FileProcessorFactory(processors);

        var result = factory.GetProcessor(FileType.Excel);

        Assert.IsType<ExcelProcessor>(result);
    }

    [Fact]
    public void GetProcessor_WithInvalidFileType_ThrowsInvalidOperationException()
    {
        var processors = new IFileProcessor[]
        {
            new CsvProcessor(),
            new ExcelProcessor()
        };
        var factory = new FileProcessorFactory(processors);

        Assert.Throws<InvalidOperationException>(() => factory.GetProcessor(FileType.Invalid));
    }

    [Fact]
    public void GetProcessor_WhenProcessorNotRegistered_ThrowsInvalidOperationException()
    {
        var processors = new IFileProcessor[]
        {
            new CsvProcessor()
        };
        var factory = new FileProcessorFactory(processors);

        var exception = Assert.Throws<InvalidOperationException>(() => factory.GetProcessor(FileType.Excel));
        Assert.Contains("not registered", exception.Message);
    }
}
