namespace Vantus.Engine.Services.AI;

public interface IEmbeddingService
{
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct);
    Task InitializeAsync();
}
