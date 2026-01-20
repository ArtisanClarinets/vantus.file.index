namespace Vantus.Engine.Services.AI;

public class OnnxEmbeddingService
{
    public async Task<byte[]> GetEmbeddingAsync(string text)
    {
        // Stub implementation
        await Task.Delay(10);
        return new byte[384 * 4]; // Mock 384-dim float vector
    }
}
