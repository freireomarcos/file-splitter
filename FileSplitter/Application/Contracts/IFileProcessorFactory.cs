using FileSplitter.Domain;

namespace FileSplitter.Application.Contracts;

public interface IFileProcessorFactory
{
    IFileProcessor GetProcessor(FileType fileType);
}
