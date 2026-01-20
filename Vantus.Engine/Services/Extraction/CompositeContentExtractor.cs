namespace Vantus.Engine.Services.Extraction;

public class CompositeContentExtractor
{
    private readonly IEnumerable<IContentExtractor> _extractors;
    private readonly ILogger<CompositeContentExtractor> _logger;

    public CompositeContentExtractor(IEnumerable<IContentExtractor> extractors, ILogger<CompositeContentExtractor> logger)
    {
        _extractors = extractors;
        _logger = logger;
    }

    public async Task<string> ExtractAsync(string filePath, CancellationToken ct)
    {
        var ext = Path.GetExtension(filePath);
        var extractor = _extractors.FirstOrDefault(e => e.CanExtract(ext));
        
        if (extractor != null)
        {
            try
            {
                _logger.LogInformation("Extracting content from {Path} using {Extractor}", filePath, extractor.GetType().Name);
                return await extractor.ExtractAsync(filePath, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract content from {Path}", filePath);
            }
        }
        
        return "";
    }
}
