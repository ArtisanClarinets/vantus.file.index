using UglyToad.PdfPig;
using Microsoft.Extensions.Logging;

namespace Vantus.Engine.Services.Extraction;

public class PdfExtractor : IContentExtractor
{
    private readonly ILogger<PdfExtractor> _logger;

    public PdfExtractor(ILogger<PdfExtractor> logger)
    {
        _logger = logger;
    }

    public bool CanExtract(string extension) => extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase);

    public Task<string> ExtractAsync(string filePath, CancellationToken ct)
    {
        return Task.Run(() =>
        {
            try
            {
                using var document = PdfDocument.Open(filePath);
                var text = new System.Text.StringBuilder();
                foreach (var page in document.GetPages())
                {
                    ct.ThrowIfCancellationRequested();
                    text.Append(page.Text);
                    text.Append(" ");
                }
                return text.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract PDF content from {Path}", filePath);
                return "";
            }
        }, ct);
    }
}
