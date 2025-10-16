using FileSplitter.Domain;

namespace FileSplitter.Application.Contracts;

public interface IUserInteraction
{
    int GetLinesPerFile();
    FileType GetFileType();
    string GetFilePath(FileType fileType);
    void DisplayWelcome();
    void DisplayProcessingStart(string fileName, int linesPerFile);
    void DisplaySuccess();
    void DisplayError(string message);
    void WaitForExit();
}
