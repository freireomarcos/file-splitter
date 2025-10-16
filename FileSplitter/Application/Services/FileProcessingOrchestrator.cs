using FileSplitter.Application.Contracts;

namespace FileSplitter.Application.Services;

public sealed class FileProcessingOrchestrator
{
    private readonly IUserInteraction _userInteraction;
    private readonly IFileProcessorFactory _processorFactory;

    public FileProcessingOrchestrator(
        IUserInteraction userInteraction,
        IFileProcessorFactory processorFactory)
    {
        _userInteraction = userInteraction;
        _processorFactory = processorFactory;
    }

    public async Task ExecuteAsync()
    {
        _userInteraction.DisplayWelcome();

        var linesPerFile = _userInteraction.GetLinesPerFile();
        var fileType = _userInteraction.GetFileType();
        var filePath = _userInteraction.GetFilePath(fileType);

        var fileName = Path.GetFileName(filePath);
        _userInteraction.DisplayProcessingStart(fileName, linesPerFile);

        var processor = _processorFactory.GetProcessor(fileType);
        await processor.ProcessAsync(filePath, linesPerFile);

        _userInteraction.DisplaySuccess();
    }
}
