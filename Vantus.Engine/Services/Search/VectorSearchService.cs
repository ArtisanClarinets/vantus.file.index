using Vantus.Engine.Data;

namespace Vantus.Engine.Services.Search;

public class VectorSearchService
{
    public Task<List<FileEntity>> SearchAsync(string query)
    {
        return Task.FromResult(new List<FileEntity>());
    }
}
