using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Vantus.Engine.Services.Extraction;

public class OfficeExtractor : IContentExtractor
{
    public bool CanExtract(string extension)
    {
        var ext = extension.ToLowerInvariant();
        return ext == ".docx";
        // TODO: Add XLSX and PPTX support
    }

    public Task<string> ExtractAsync(string filePath, CancellationToken ct)
    {
        return Task.Run(() =>
        {
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            if (ext == ".docx") return ExtractDocx(filePath);
            return "";
        }, ct);
    }

    private string ExtractDocx(string filePath)
    {
        try
        {
            using var doc = WordprocessingDocument.Open(filePath, false);
            return doc.MainDocumentPart?.Document.Body?.InnerText ?? "";
        }
        catch { return ""; }
    }
}
