namespace FileSplitter.Features.Csv.Validators;

public static class CsvLineValidator
{
    public static bool IsEmpty(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return true;

        var cleaned = line
            .Replace(",", "")
            .Replace(";", "")
            .Replace("\t", "")
            .Trim();

        return string.IsNullOrWhiteSpace(cleaned);
    }
}
