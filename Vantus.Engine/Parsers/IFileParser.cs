using UglyToad.PdfPig;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Tesseract;

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
        return Task.Run(() =>
        {
            try
            {
                using var document = PdfDocument.Open(filePath);
                var pages = document.GetPages().Select(p => p.Text);
                return string.Join(Environment.NewLine, pages);
            }
            catch
            {
                return string.Empty;
            }
        });
    }
}

public class OfficeParser : IFileParser
{
    public bool CanParse(string extension) => extension.Equals(".docx", StringComparison.OrdinalIgnoreCase);

    public Task<string> ParseAsync(string filePath)
    {
        return Task.Run(() =>
        {
            try
            {
                using var doc = WordprocessingDocument.Open(filePath, false);
                var body = doc.MainDocumentPart?.Document.Body;
                return body?.InnerText ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        });
    }
}

public class ImageParser : IFileParser
{
    public bool CanParse(string extension) =>
        extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".tif", StringComparison.OrdinalIgnoreCase);

    public Task<string> ParseAsync(string filePath)
    {
        return Task.Run(() =>
        {
            try
            {
                // Note: Tesseract requires 'tessdata' folder with language files in the app directory.
                // For this environment, we assume it's present or fail gracefully.
                var tessDataPath = Path.Combine(AppContext.BaseDirectory, "tessdata");
                if (!Directory.Exists(tessDataPath))
                {
                    // Fallback if no tessdata
                    return $"[OCR Unavailable: Missing tessdata] {Path.GetFileName(filePath)}";
                }

                using var engine = new TesseractEngine(tessDataPath, "eng", EngineMode.Default);
                using var img = Pix.LoadFromFile(filePath);
                using var page = engine.Process(img);
                return page.GetText();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        });
    }
}
