namespace Vantus.Engine.Services.Extraction;

public interface IContentExtractor
{
    bool CanExtract(string extension);
    Task<string> ExtractAsync(string filePath, CancellationToken ct);
}
