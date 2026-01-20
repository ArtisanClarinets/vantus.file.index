namespace Vantus.Engine.Parsers;

public interface IFileParser
{
    bool CanParse(string extension);
    Task<string> ParseAsync(string filePath);
}

public class TextParser : IFileParser
{
    public bool CanParse(string extension) => extension.Equals(".txt", StringComparison.OrdinalIgnoreCase) || extension.Equals(".md", StringComparison.OrdinalIgnoreCase) || extension.Equals(".cs", StringComparison.OrdinalIgnoreCase);

    public async Task<string> ParseAsync(string filePath)
    {
        try { return await File.ReadAllTextAsync(filePath); }
        catch { return string.Empty; }
    }
}

public class PdfParser : IFileParser
{
    public bool CanParse(string extension) => extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase);

    public Task<string> ParseAsync(string filePath)
    {
        // Placeholder for PDF parsing logic (e.g. PdfPig or iText)
        // For now, return a placeholder
        return Task.FromResult($"[PDF Content Placeholder for {Path.GetFileName(filePath)}]");
    }
}

public class ImageParser : IFileParser
{
    public bool CanParse(string extension) =>
        extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".png", StringComparison.OrdinalIgnoreCase);

    public Task<string> ParseAsync(string filePath)
    {
        // Placeholder for OCR / EXIF extraction
        return Task.FromResult($"[Image Metadata Placeholder for {Path.GetFileName(filePath)}]");
    }
}
