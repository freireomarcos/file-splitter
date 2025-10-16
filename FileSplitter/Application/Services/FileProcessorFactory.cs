using FileSplitter.Application.Contracts;
using FileSplitter.Domain;

namespace FileSplitter.Application.Services;

public sealed class FileProcessorFactory : IFileProcessorFactory
{
    private readonly IEnumerable<IFileProcessor> _processors;

    public FileProcessorFactory(IEnumerable<IFileProcessor> processors)
    {
        _processors = processors;
    }

    public IFileProcessor GetProcessor(FileType fileType)
    {
        var processorType = fileType switch
        {
            FileType.Csv => typeof(Features.Csv.CsvProcessor),
            FileType.Excel => typeof(Features.Excel.ExcelProcessor),
            _ => throw new InvalidOperationException($"No processor found for file type: {fileType}")
        };

        var processor = _processors.FirstOrDefault(p => p.GetType() == processorType);

        if (processor == null)
            throw new InvalidOperationException($"Processor of type {processorType.Name} is not registered.");

        return processor;
    }
}
