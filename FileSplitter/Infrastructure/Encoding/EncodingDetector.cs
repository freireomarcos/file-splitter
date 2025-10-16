using System.Text;

namespace FileSplitter.Infrastructure.Encoding;

public static class EncodingDetector
{
    public static System.Text.Encoding Detect(string filePath)
    {
        var buffer = new byte[4];
        
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var bytesRead = stream.Read(buffer, 0, 4);
        
        if (bytesRead < 2)
            return System.Text.Encoding.UTF8;

        if (HasUtf8Bom(buffer))
            return System.Text.Encoding.UTF8;
        
        if (HasUtf32LeBom(buffer))
            return System.Text.Encoding.UTF32;
        
        if (HasUnicodeLeBom(buffer))
            return System.Text.Encoding.Unicode;
        
        if (HasUnicodeBeBom(buffer))
            return System.Text.Encoding.BigEndianUnicode;

        return TryReadAsUtf8(filePath);
    }

    private static bool HasUtf8Bom(byte[] buffer) =>
        buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF;

    private static bool HasUtf32LeBom(byte[] buffer) =>
        buffer[0] == 0xFF && buffer[1] == 0xFE && buffer[2] == 0x00 && buffer[3] == 0x00;

    private static bool HasUnicodeLeBom(byte[] buffer) =>
        buffer[0] == 0xFF && buffer[1] == 0xFE;

    private static bool HasUnicodeBeBom(byte[] buffer) =>
        buffer[0] == 0xFE && buffer[1] == 0xFF;

    private static System.Text.Encoding TryReadAsUtf8(string filePath)
    {
        try
        {
            File.ReadAllText(filePath, System.Text.Encoding.UTF8);
            return System.Text.Encoding.UTF8;
        }
        catch
        {
            return System.Text.Encoding.Default;
        }
    }
}
